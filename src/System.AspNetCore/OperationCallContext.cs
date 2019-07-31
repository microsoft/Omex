// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.Omex.System.Context;

namespace Microsoft.Omex.System.AspNetCore
{
	/// <summary>
	/// Context data specific to the current call connected with an OperationContext (WCF)
	/// </summary>
	public class OperationCallContext : ICallContext
	{

		/// <summary>
		/// Used when there is no operation call context
		/// </summary>
		private readonly ICallContext m_backupCallContext = new ThreadCallContext();


		/// <summary>
		/// Dictionary of data stored on the call context
		/// </summary>
		public IDictionary<string, object> Data
		{
			get
			{
				OperationContext context = OperationContext.Current;
				if (context != null)
				{
					Stack<Dictionary<string, object>> dataStack;
					if (s_contextData.TryGetValue(context, out dataStack) && dataStack.Count > 0)
					{
						Dictionary<string, object> data = dataStack.Peek();
						if (data == null)
						{
							data = new Dictionary<string, object>();
							dataStack.Pop();
							dataStack.Push(data);
						}

						return data;
					}
					else
					{
						throw new InvalidOperationException("Attempting to access data for an OperationCallContext that has not been started");
					}
				}

				return m_backupCallContext.Data;
			}
		}


		/// <summary>
		/// Dictionary of data stored on the call context and shared between derived call contexts
		/// </summary>
		public ConcurrentDictionary<string, object> SharedData
		{
			get
			{
				OperationContext context = OperationContext.Current;
				if (context != null)
				{
					return s_sharedContextData.GetOrAdd(context, _ => new ConcurrentDictionary<string, object>(StringComparer.Ordinal));
				}

				return m_backupCallContext.SharedData;
			}
		}


		/// <summary>
		/// Add data to context
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="value">value</param>
		public void AddContextValue(string key, object value)
		{
		}


		/// <summary>
		/// Start the call context
		/// </summary>
		/// <returns>id of the context node</returns>
		public Guid? StartCallContext()
		{
			OperationContext context = OperationContext.Current;
			if (context != null)
			{
				Stack<Dictionary<string, object>> dataStack = s_contextData.GetOrAdd(context, (operationContext) => new Stack<Dictionary<string, object>>());
				dataStack.Push(null);
			}
			else
			{
				return m_backupCallContext.StartCallContext();
			}

			return null;
		}


		/// <summary>
		/// End the call context
		/// </summary>
		/// <param name="threadId">id of the thread on which to end the context</param>
		/// <param name="nodeId">id of the context node</param>
		public void EndCallContext(int? threadId = null, Guid? nodeId = null)
		{
			OperationContext context = OperationContext.Current;
			if (context != null)
			{
				Stack<Dictionary<string, object>> dataStack;
				if (s_contextData.TryGetValue(context, out dataStack) && dataStack.Count > 0)
				{
					dataStack.Pop();
					if (dataStack.Count == 0)
					{
						s_contextData.TryRemove(context, out dataStack);

						ConcurrentDictionary<string, object> _;
						s_sharedContextData.TryRemove(context, out _);
					}

					return;
				}
				else
				{
					throw new InvalidOperationException("Attempting to end an OperationCallContext that has not been started");
				}
			}

			m_backupCallContext.EndCallContext(threadId, nodeId);
		}


		/// <summary>
		/// Get the existing call context if there is one
		/// </summary>
		/// <param name="threadId">id of the thread from which to take the context</param>
		/// <returns>the call context that is being used</returns>
		public ICallContext ExistingCallContext(int? threadId = null)
		{
			OperationContext context = OperationContext.Current;
			if (context != null)
			{
				Stack<Dictionary<string, object>> dataStack;
				if (s_contextData.TryGetValue(context, out dataStack) && dataStack.Count > 0)
				{
					return this;
				}

				return null;
			}

			return m_backupCallContext.ExistingCallContext(threadId);
		}


		/// <summary>
		/// Data stored per operation context
		/// </summary>
		/// <remarks>Concurrency level is set to 100 instead of the default 4*number of processors. This gives a more
		/// granular lock for processors using high contention. The initial capacity is set to avoid capacity to grow
		/// which is expensive.
		/// </remarks>
		private static readonly ConcurrentDictionary<OperationContext, Stack<Dictionary<string, object>>> s_contextData =
			new ConcurrentDictionary<OperationContext, Stack<Dictionary<string, object>>>(100, 1000);


		/// <summary>
		/// Data stored per operation context and shared between all child contexts
		/// </summary>
		/// <remarks>Concurrency level is set to 100 instead of the default 4*number of processors. This gives a more
		/// granular lock for processors using high contention. The initial capacity is set to avoid capacity to grow
		/// which is expensive.
		/// </remarks>
		private static readonly ConcurrentDictionary<OperationContext, ConcurrentDictionary<string, object>> s_sharedContextData =
			new ConcurrentDictionary<OperationContext, ConcurrentDictionary<string, object>>(100, 1000);
	}
}

