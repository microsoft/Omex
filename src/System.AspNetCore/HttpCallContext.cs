// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Omex.System.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Omex.System.Diagnostics;

namespace Microsoft.Omex.System.AspNetCore
{
	/// <summary>
	/// Context data stored on HttpContext
	/// </summary>
	public class HttpCallContext : ICallContext
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="useLogicalCallContext">Should LogicalCallContext be used as backup context</param>
		/// <param name="machineInformation"></param>
		public HttpCallContext(bool useLogicalCallContext = true, IMachineInformation machineInformation = null)
		{
			m_backupCallContext = useLogicalCallContext ? (ICallContext)new LogicalCallContext(machineInformation) : new OperationCallContext();
		}


		/// <summary>
		/// Constructor
		/// </summary>
		public HttpCallContext() : this(true)
		{
		}


		/// <summary>
		/// Used when there is no http call context
		/// </summary>
		private readonly ICallContext m_backupCallContext;


		/// <summary>
		/// Dictionary of data stored on the call context
		/// </summary>
		public IDictionary<string, object> Data
		{
			get
			{
				HttpContext current = HttpContextWrapper.Current;
				if (current != null)
				{
					if (current.Items.ContainsKey(UseBackupKey))
					{
						return m_backupCallContext.Data;
					}

					if (current.Items.ContainsKey(DictionaryKey))
					{
						if (!(current.Items[DictionaryKey] is Dictionary<string, object> dictionary))
						{
							dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
							current.Items[DictionaryKey] = dictionary;
						}

						return dictionary;
					}
					else
					{
						throw new InvalidOperationException("Attempting to access data for a HttpCallContext that has not been started");
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
				HttpContext current = HttpContextWrapper.Current;
				if (current != null)
				{
					if (current.Items.ContainsKey(UseBackupKey))
					{
						return m_backupCallContext.SharedData;
					}

					return current.Items[DataKey] as ConcurrentDictionary<string, object>;
				}

				return m_backupCallContext.SharedData;
			}
		}


		/// <summary>
		/// Set data to http context
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="value">value</param>
		public void AddContextValue(string key, object value)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return;
			}

			HttpContext current = HttpContextWrapper.Current;
			if (current != null && !current.Items.ContainsKey(UseBackupKey))
			{
				current.Items[key.Trim()] = value;
			}
			else
			{
				m_backupCallContext.AddContextValue(key, value);
			}
		}


		/// <summary>
		/// Start the call context
		/// </summary>
		/// <returns>id of the context node</returns>
		public Guid? StartCallContext()
		{
			HttpContext current = HttpContextWrapper.Current;
			if (current != null)
			{
				if (current.Items.ContainsKey(UseBackupKey))
				{
					return m_backupCallContext.StartCallContext();
				}

				if (!current.Items.ContainsKey(DataKey))
				{
					current.Items[DataKey] = new ConcurrentDictionary<string, object>(StringComparer.Ordinal);
				}

				if (current.Items.ContainsKey(DictionaryKey))
				{
					Stack<Dictionary<string, object>> dataStack = null;
					if (current.Items.ContainsKey(ContextStack))
					{
						dataStack = current.Items[ContextStack] as Stack<Dictionary<string, object>>;
					}
					else
					{
						dataStack = new Stack<Dictionary<string, object>>();
						current.Items[ContextStack] = dataStack;
					}

					dataStack.Push(current.Items[DictionaryKey] as Dictionary<string, object>);
				}

				current.Items[DictionaryKey] = null;
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
			HttpContext current = HttpContextWrapper.Current;
			if (current != null)
			{
				if (current.Items.ContainsKey(UseBackupKey))
				{
					m_backupCallContext.EndCallContext(threadId, nodeId);
					return;
				}

				if (current.Items.ContainsKey(DictionaryKey))
				{
					current.Items.Remove(DictionaryKey);

					if (current.Items.ContainsKey(ContextStack))
					{
						if (current.Items[ContextStack] is Stack<Dictionary<string, object>> dataStack && dataStack.Count > 0)
						{
							current.Items[DictionaryKey] = dataStack.Pop();
						}
					}
				}
#if DEBUG
				else
				{
					throw new InvalidOperationException("Attempting to end a HttpCallContext that has not been started.");
				}
#endif
			}
			else
			{
				m_backupCallContext.EndCallContext(threadId, nodeId);
			}
		}


		/// <summary>
		/// Get the existing call context if there is one
		/// </summary>
		/// <param name="threadId">id of the thread from which to take the context</param>
		/// <returns>the call context that is being used</returns>
		public ICallContext ExistingCallContext(int? threadId = null)
		{
			HttpContext current = HttpContextWrapper.Current;
			if (current != null)
			{
				if (current.Items.ContainsKey(UseBackupKey))
				{
					return m_backupCallContext.ExistingCallContext(threadId);
				}

				if (current.Items.ContainsKey(DictionaryKey))
				{
					return this;
				}

				return null;
			}

			return m_backupCallContext.ExistingCallContext(threadId);
		}


		/// <summary>
		/// Move data to backup context
		/// </summary>
		public void MoveCurrentContextToBackup()
		{
			HttpContext current = HttpContextWrapper.Current;
			if (current != null && m_backupCallContext is LogicalCallContext)
			{
				IDictionary<string, object> dictionary = GetData(current);
				ConcurrentDictionary<string, object> sharedDictionary = GetSharedData(current);

				// Start call context on the backup context
				int threadId = Thread.CurrentThread.ManagedThreadId;
				Guid? nodeId = m_backupCallContext.StartCallContext();

				foreach (KeyValuePair<string, object> item in dictionary)
				{
					m_backupCallContext.Data.Add(item);
				}

				foreach (KeyValuePair<string, object> item in sharedDictionary)
				{
					m_backupCallContext.SharedData.AddOrUpdate(item.Key, _ => item.Value, (_, __) => item.Value);
				}

				// Mark this context as copied over to backup
				// This will result in a fallthrough to backup context
				current.Items[UseBackupKey] = Tuple.Create(threadId, nodeId);
			}
		}


		/// <summary>
		/// Retrieve data from backup context
		/// </summary>
		public void RetrieveCurrentContextFromBackup()
		{
			HttpContext current = HttpContextWrapper.Current;
			if (current != null && m_backupCallContext is LogicalCallContext)
			{
				// Do not use Data or SharedData to avoid falling through to BackupContext
				IDictionary<string, object> dictionary = GetData(current);
				ConcurrentDictionary<string, object> sharedDictionary = GetSharedData(current);

				foreach (KeyValuePair<string, object> item in m_backupCallContext.Data)
				{
					dictionary[item.Key] = item.Value;
				}

				foreach (KeyValuePair<string, object> item in m_backupCallContext.SharedData)
				{
					sharedDictionary.AddOrUpdate(item.Key, _ => item.Value, (_, __) => item.Value);
				}

				// End call context on the backup context
				Tuple<int, Guid?> contextKey = current.Items[UseBackupKey] as Tuple<int, Guid?>;
				m_backupCallContext.EndCallContext(contextKey?.Item1, contextKey?.Item2);

				// Unmark this context
				current.Items.Remove(UseBackupKey);
			}
		}


		private static IDictionary<string, object> GetData(HttpContext currentContext)
		{
			Dictionary<string, object> dictionary = null;
			if (currentContext.Items.ContainsKey(DictionaryKey))
			{
				dictionary = currentContext.Items[DictionaryKey] as Dictionary<string, object>;
			}

			if (dictionary == null)
			{
				dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
				currentContext.Items[DictionaryKey] = dictionary;
			}

			return dictionary;
		}


		private static ConcurrentDictionary<string, object> GetSharedData(HttpContext currentContext)
		{
			ConcurrentDictionary<string, object> sharedDictionary = null;
			if (currentContext.Items.ContainsKey(DataKey))
			{
				sharedDictionary = currentContext.Items[DataKey] as ConcurrentDictionary<string, object>;
			}

			if (sharedDictionary == null)
			{
				sharedDictionary = new ConcurrentDictionary<string, object>(StringComparer.Ordinal);
				currentContext.Items[DataKey] = sharedDictionary;
			}

			return sharedDictionary;
		}


		/// <summary>
		/// Key used to signify that the context should fall through and use backup context instead
		/// </summary>
		private const string UseBackupKey = "ThreadCallContext.UseBackup";


		/// <summary>
		/// Key used to store data shared between multiple related contexts
		/// </summary>
		private const string DataKey = "ThreadCallContext.Data";


		/// <summary>
		/// Key used to store data on the underlying context
		/// </summary>
		private const string DictionaryKey = "ThreadCallContext.Dictionary";


		/// <summary>
		/// Key used to store nested call contexts
		/// </summary>
		private const string ContextStack = "ThreadCallContext.Stack";
	}
}
