// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Context;
using Microsoft.Omex.System.Diagnostics;

namespace Microsoft.Omex.System.AspNetCore
{
	/// <summary>
	/// Context data stored on LogicalCallContext
	/// </summary>
	public abstract class LogicalCallContextBase : ICallContext
	{

		public LogicalCallContextBase(IMachineInformation machineInformation)
		{
			MachineInformation = machineInformation;
		}

		/// <summary>
		/// Dictionary of data stored on the call context
		/// </summary>
		public IDictionary<string, object> Data
		{
			get
			{
				if (ContextData == null)
				{
					throw new InvalidOperationException("Attempting to access data for a LogicalCallContext that has not been started");
				}

				IImmutableDictionary<string, object> dictionary = ContextDictionary;
				if (dictionary == null)
				{
					dictionary = new SerializableImmutableDictionary<string, object>(ImmutableDictionary<string, object>.Empty.WithComparers(StringComparer.Ordinal));
					ContextDictionary = dictionary;
				}

				return new LogicalCallContextDictionary(this, dictionary);
			}
		}


		/// <summary>
		/// Dictionary of data stored on the call context and shared between derived call contexts
		/// </summary>
		public ConcurrentDictionary<string, object> SharedData
		{
			get
			{
				if (ContextData == null)
				{
					throw new InvalidOperationException("Attempting to access shared data for a LogicalCallContext that has not been started");
				}

				return ContextData;
			}
		}


		/// <summary>
		/// Set data to http context
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
			if (ContextData == null)
			{
				ContextData = new ConcurrentDictionary<string, object>(StringComparer.Ordinal);
			}

			if (ContextDictionary != null)
			{
				IImmutableStack<IImmutableDictionary<string, object>> dataStack = null;
				if (ContextStack != null)
				{
					dataStack = ContextStack;
				}
				else
				{
					dataStack = new SerializableImmutableStack<IImmutableDictionary<string, object>>(ImmutableStack<IImmutableDictionary<string, object>>.Empty);
					ContextStack = dataStack;
				}

				ContextStack = dataStack.Push(ContextDictionary);
			}

			ContextDictionary = new SerializableImmutableDictionary<string, object>(ImmutableDictionary<string, object>.Empty.WithComparers(StringComparer.Ordinal));
			return null;
		}


		/// <summary>
		/// End the call context
		/// </summary>
		/// <param name="threadId">id of the thread on which to end the context</param>
		/// <param name="nodeId">id of the context node</param>
		public void EndCallContext(int? threadId = null, Guid? nodeId = null)
		{
			if (ContextDictionary != null)
			{
				ContextDictionary = null;

				if (ContextStack != null)
				{
					IImmutableStack<IImmutableDictionary<string, object>> dataStack = ContextStack;
					if (dataStack != null && !dataStack.IsEmpty)
					{
						ContextDictionary = dataStack.Peek();
						ContextStack = dataStack.Pop();
					}
				}
			}
			else if (!s_testEnvironment.Value)
			{
				ULSLogging.LogTraceTag(0x23847117 /* tag_97hex */, Categories.Infrastructure, Levels.Warning,
					"Attempting to end a LogicalCallContext that has not been started.");
			}
			else
			{
				throw new InvalidOperationException("Attempting to end a LogicalCallContext that has not been started.");
			}
		}


		public static IMachineInformation MachineInformation { get; set; }


		/// <summary>
		/// Current environment
		/// </summary>
		private static readonly Lazy<bool> s_testEnvironment = new Lazy<bool>(() =>
			MachineInformation.IsPrivateDeployment,
			LazyThreadSafetyMode.PublicationOnly);


		/// <summary>
		/// Get the existing call context if there is one
		/// </summary>
		/// <param name="threadId">id of the thread from which to take the context</param>
		/// <returns>the call context that is being used</returns>
		public ICallContext ExistingCallContext(int? threadId = null)
		{
			if (ContextDictionary != null)
			{
				return this;
			}

			return null;
		}


		/// <summary>Context dictionary</summary>
		protected abstract ConcurrentDictionary<string, object> ContextData { get; set; }


		/// <summary>Context dictionary</summary>
		protected abstract IImmutableDictionary<string, object> ContextDictionary { get; set; }


		/// <summary> /// Gets or sets the context stack /// </summary>
		protected abstract IImmutableStack<IImmutableDictionary<string, object>> ContextStack { get; set; }


		/// <summary>
		/// Mutable dictionary which ensures that all changes are persisted correctly
		/// </summary>
		private class LogicalCallContextDictionary : IDictionary<string, object>
		{
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="context">Instance of logical context to set dictionary</param>
			/// <param name="innerDictionary">Items storage</param>
			public LogicalCallContextDictionary(LogicalCallContextBase context, IImmutableDictionary<string, object> innerDictionary)
			{
				m_context = context;
				m_innerDictionary = innerDictionary;
			}


			/// <summary>
			/// Items storage
			/// </summary>
			private IImmutableDictionary<string, object> m_innerDictionary;


			/// <summary>
			/// Returns an enumerator that iterates through the immutable dictionary.
			/// </summary>
			/// <returns></returns>
			public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => m_innerDictionary.GetEnumerator();


			/// <summary>
			/// Returns an enumerator that iterates through the immutable dictionary.
			/// </summary>
			/// <returns></returns>
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


			/// <summary>
			/// Adds an element with the provided key and value to the immutable dictionary.
			/// </summary>
			/// <param name="item"></param>
			public void Add(KeyValuePair<string, object> item)
			{
				Add(item.Key, item.Value);
			}


			/// <summary>
			/// Clears this instance.
			/// </summary>
			public void Clear()
			{
				IImmutableDictionary<string, object> dict = m_innerDictionary = m_innerDictionary.Clear();
				m_context.ContextDictionary = dict;
			}


			/// <summary>
			/// Determines whether this immutable dictionary contains the specified key/value pair.
			/// </summary>
			/// <param name="item"></param>
			public bool Contains(KeyValuePair<string, object> item) => m_innerDictionary.Contains(item);


			/// <summary>
			/// Copies the elements of the dictionary to an array, starting at a particular array index.
			/// </summary>
			/// <param name="array">Array</param>
			/// <param name="arrayIndex">Array index</param>
			public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
			{
				((IDictionary<string, object>)m_innerDictionary).CopyTo(array, arrayIndex);
			}


			/// <summary>
			/// Removes an item from the dictionary
			/// </summary>
			/// <param name="item">Item to remove</param>
			/// <returns>True if items was removed from the dictionary</returns>
			public bool Remove(KeyValuePair<string, object> item) => Remove(item.Key);


			/// <summary>
			/// Gets the number of elements contained in the dictionary
			/// </summary>
			public int Count => m_innerDictionary.Count;


			/// <summary>
			/// Gets a value indicating whether the dictionary is read only
			/// </summary>
			public bool IsReadOnly => false;


			/// <summary>
			/// Determines whether the dictinoary contains an element with the specified key.
			/// </summary>
			/// <param name="key">Key</param>
			/// <returns>True if dictionary contains specified key</returns>
			public bool ContainsKey(string key) => m_innerDictionary.ContainsKey(key);


			/// <summary>
			/// Adds an element with the provided key and value to the Dictionary
			/// </summary>
			/// <param name="key">Key</param>
			/// <param name="value">Value</param>
			public void Add(string key, object value)
			{
				m_innerDictionary = m_innerDictionary.SetItem(key, value);
				m_context.ContextDictionary = m_innerDictionary;
			}


			/// <summary>
			/// Removes the element with the specified key from the dictionary
			/// </summary>
			/// <param name="key">Key</param>
			/// <returns>True if item was removed</returns>
			public bool Remove(string key)
			{
				bool result = ContainsKey(key);
				if (!result)
				{
					return false;
				}

				m_innerDictionary = m_innerDictionary.Remove(key);
				m_context.ContextDictionary = m_innerDictionary;
				return true;
			}


			/// <summary>
			/// Gets the value associated with the specified key.
			/// </summary>
			/// <param name="key">Key</param>
			/// <param name="value">Value</param>
			/// <returns>True if key exisited in the dictionary</returns>
			public bool TryGetValue(string key, out object value) => m_innerDictionary.TryGetValue(key, out value);


			/// <summary>
			/// Gets or sets the element with the specified key.
			/// </summary>
			/// <param name="key">Key to set or get</param>
			/// <returns>Value associated with the key</returns>
			public object this[string key]
			{
				get
				{
					return m_innerDictionary[key];
				}
				set
				{
					Add(key, value);
				}
			}


			/// <summary>
			/// Collection containing keys of the dictionary
			/// </summary>
			public ICollection<string> Keys => m_innerDictionary.Keys.ToList();


			/// <summary>
			/// Collection containing values of the dictionary
			/// </summary>
			public ICollection<object> Values => m_innerDictionary.Values.ToList();


			private readonly LogicalCallContextBase m_context;
		}
	}
}


