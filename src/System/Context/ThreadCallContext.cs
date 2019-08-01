// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;

namespace Microsoft.Omex.System.Context
{
	/// <summary>
	/// Context data stored on CallContext
	/// </summary>
	public class ThreadCallContext : ICallContext
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public ThreadCallContext()
			: this(null)
		{
		}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="threadId">Id of the thread</param>
		public ThreadCallContext(int? threadId = null) => m_id = threadId;


		/// <summary>
		/// Dictionary of thread context data. Managed thread id is used as a key.
		/// </summary>
		/// <remarks>Concurrency level is set to 100 instead of the default 4*number of processors. This gives a more
		/// granular lock for processors using high contention. The initial capacity is set to avoid capacity to grow
		/// which is expensive.
		/// </remarks>
		private static readonly ConcurrentDictionary<int, ThreadCallContextData> s_threads = new ConcurrentDictionary<int, ThreadCallContextData>(100, 1000);


		/// <summary>
		/// Dictionary of data stored on the call context
		/// </summary>
		public IDictionary<string, object> Data => SharedData;


		/// <summary>
		/// Dictionary of data stored on the call context and shared between derived call contexts
		/// </summary>
		public ConcurrentDictionary<string, object> SharedData => s_threads.GetOrAdd(ID, _ => new ThreadCallContextData()).SharedData;


		/// <summary>
		/// Add data to context
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="value">value</param>
		public void AddContextValue(string key, object value) => SharedData?.AddOrUpdate(key, value, (_, __) => value);


		/// <summary>
		/// Start the call context
		/// </summary>
		/// <returns>id of the context node</returns>
		public Guid? StartCallContext() => s_threads.GetOrAdd(ID, _ => new ThreadCallContextData()).StartCallContext();


		/// <summary>
		/// Id of the current thread. Used to access thread specific data
		/// </summary>
		private int ID => m_id ?? Thread.CurrentThread.ManagedThreadId;


		/// <summary>
		/// Id override
		/// </summary>
		private readonly int? m_id;


		/// <summary>
		/// End the call context
		/// </summary>
		/// <param name="threadId">id of the thread on which to end the context</param>
		/// <param name="nodeId">id of the context node</param>
		/// <exception cref="InvalidOperationException">If there is no valid call context to end</exception>
		public void EndCallContext(int? threadId = null, Guid? nodeId = null)
		{
			int currentId = threadId ?? ID;

			ThreadCallContextData threadCallContextData = s_threads.GetOrAdd(currentId, _ => new ThreadCallContextData());
			threadCallContextData.EndCallContext(nodeId);

			if (!threadCallContextData.Exists)
			{
				ThreadCallContextData _;
				s_threads.TryRemove(currentId, out _);
			}
		}


		/// <summary>
		/// Get the existing call context if there is one
		/// </summary>
		/// <param name="threadId">id of the thread from which to take the context</param>
		/// <returns>the call context that is being used</returns>
		public ICallContext ExistingCallContext(int? threadId = null)
		{
			if (threadId.HasValue)
			{
				return new ThreadCallContext(threadId).ExistingCallContext();
			}

			// Check if thread id exists in the dictionary to avoid initialization
			int currentId = ID;
			if (s_threads.ContainsKey(currentId))
			{
				return s_threads.GetOrAdd(currentId, _ => new ThreadCallContextData()).Exists ? this : null;
			}

			return null;
		}


		/// <summary>
		/// Class which stores thread specific data about a call context
		/// </summary>
		private class ThreadCallContextData
		{
			/// <summary>
			/// Data for the current call context
			/// </summary>
			private ConcurrentDictionary<string, object> m_data = new ConcurrentDictionary<string, object>(StringComparer.Ordinal);


			/// <summary>
			/// Stack of nested call contexts
			/// </summary>
			private readonly LinkedList<ConcurrentDictionary<string, object>> m_dataStack = new LinkedList<ConcurrentDictionary<string, object>>();


			/// <summary>
			/// Mapping between a GUID (context node id) and its call context
			/// </summary>
			private readonly IOrderedDictionary m_nodesDictionary = new OrderedDictionary();


			/// <summary>
			/// List of GUIDs (context node id) for call contexts which have ended asynchronously and can be safely removed
			/// </summary>
			private readonly ConcurrentQueue<Guid> m_nodesToRemove = new ConcurrentQueue<Guid>();


			/// <summary>
			/// Current level of nesting of call contexts
			/// </summary>
			private int m_stackCount = 0;


			/// <summary>
			/// Is there any active call context
			/// </summary>
			public bool Exists => m_stackCount > 0;


			/// <summary>
			/// Dictionary of data stored on the call context
			/// </summary>
			public ConcurrentDictionary<string, object> SharedData
			{
				get
				{
#if DEBUG
					if (m_stackCount <= 0)
					{
						throw new InvalidOperationException();
					}
#endif

					if (m_data == null)
					{
						m_data = new ConcurrentDictionary<string, object>(StringComparer.Ordinal);
					}

					return m_data;
				}
			}


			/// <summary>
			/// Start call context
			/// </summary>
			/// <returns>id of the context node</returns>
			public Guid? StartCallContext()
			{
				RemoveEndedCallContexts();

				Guid? nodeGuid = null;
				if (m_stackCount > 0)
				{
					LinkedListNode<ConcurrentDictionary<string, object>> dataNode = new LinkedListNode<ConcurrentDictionary<string, object>>(SharedData);
					m_dataStack.AddFirst(dataNode);
					m_data = null;

					nodeGuid = Guid.NewGuid();
					lock (m_nodesDictionary)
					{
						m_nodesDictionary.Add(nodeGuid, dataNode);
					}
				}


				Interlocked.Increment(ref m_stackCount);
				return nodeGuid;
			}


			/// <summary>
			/// End call context
			/// </summary>
			/// <param name="nodeId">Id of the context node</param>
			/// <remarks>This method may be called by multiple threads</remarks>
			public void EndCallContext(Guid? nodeId)
			{
				if (nodeId.HasValue)
				{
					// We may be accessing this method from a different thread
					// Enqueue this node for removal once an end call context is called by this thread
					m_nodesToRemove.Enqueue(nodeId.Value);
				}
				else
				{
					if (!Exists)
					{
						RemoveEndedCallContexts();
#if DEBUG
						throw new InvalidOperationException("There is no active call context to end.");
#else
						return;
#endif
					}

					if (m_stackCount == 1)
					{
						m_data = null;
					}
					else
					{
						Guid lastGuid;
						lock (m_nodesDictionary)
						{
							lastGuid = (Guid)m_nodesDictionary.Cast<DictionaryEntry>().Last().Key;
						}

						m_nodesToRemove.Enqueue(lastGuid);
					}

					RemoveEndedCallContexts();
				}

				Interlocked.Decrement(ref m_stackCount);
			}


			/// <summary>
			/// Remove call context which ended
			/// </summary>
			/// <remarks>This method does not decrement stack count, because it was decremented when a context was queued for removal</remarks>
			private void RemoveEndedCallContexts()
			{
				while (m_nodesToRemove.TryDequeue(out Guid guidToRemove))
				{
					LinkedListNode<ConcurrentDictionary<string, object>> node;
					lock (m_nodesDictionary)
					{
						node =
							m_nodesDictionary[guidToRemove] as LinkedListNode<ConcurrentDictionary<string, object>>;
						m_nodesDictionary.Remove(guidToRemove);
					}

					if (node == m_dataStack.First)
					{
						m_data = node.Value;
					}

					if (node != null)
					{
						m_dataStack.Remove(node);
					}
				}
			}
		}
	}
}