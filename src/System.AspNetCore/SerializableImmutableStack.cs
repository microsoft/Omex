// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Microsoft.Omex.System.Validation;
using Microsoft.Omex.System.Logging;

namespace Microsoft.Omex.System.AspNetCore
{
	/// <summary>
	/// Serializable implementation of the ImmutableStack
	/// </summary>
	/// <typeparam name="TValue">Value type</typeparam>
	[Serializable]
	public class SerializableImmutableStack<TValue> : MarshalByRefObject, IImmutableStack<TValue>, ISerializable, IDeserializationCallback
	{
		[XmlIgnore]
		[NonSerialized]
		private IImmutableStack<TValue> m_store;


		/// <summary>
		/// Stores serilization info received in the constructor until
		/// the deserilization callback is called and the object can be deserilized
		/// </summary>
		[XmlIgnore]
		[NonSerialized]
		private SerializationInfo m_serializationInfo;


		private IImmutableStack<TValue> Store => m_store;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="store">Data store</param>
		public SerializableImmutableStack(IImmutableStack<TValue> store)
		{
			Code.ExpectsArgument(store, nameof(store), TaggingUtilities.ReserveTag(0));

			m_store = store;
		}


		/// <summary>
		/// Constructor
		/// </summary>
		public SerializableImmutableStack()
		{
			m_store = ImmutableStack<TValue>.Empty;
		}


		/// <summary>
		/// Constructor used for deserialization.
		/// </summary>
		/// <remarks>Stores serialization info in a field to allow deserialization callback to complete deserialization</remarks>
		protected SerializableImmutableStack(SerializationInfo info, StreamingContext context)
		{
			Code.ExpectsArgument(info, nameof(info), TaggingUtilities.ReserveTag(0));

			m_serializationInfo = info;
		}


		/// <summary>
		/// Returns an enumerator that iterates through the immutable stack.
		/// </summary>
		/// <returns>Enumerator</returns>
		public IEnumerator<TValue> GetEnumerator() => Store.GetEnumerator();


		/// <summary>
		/// Returns an enumerator that iterates through the immutable stack.
		/// </summary>
		/// <returns>Enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


		/// <summary>
		/// Removes all objects from the immutable stack.
		/// </summary>
		/// <returns>Empty stack</returns>
		public IImmutableStack<TValue> Clear()
			=> new SerializableImmutableStack<TValue>(Store.Clear());


		/// <summary>
		/// Inserts an object at the top of the immutable stack and returns the new stack.
		/// </summary>
		/// <param name="value">Value to push</param>
		/// <returns>Immutable stack with the value at the top</returns>
		public IImmutableStack<TValue> Push(TValue value)
			=> new SerializableImmutableStack<TValue>(Store.Push(value));


		/// <summary>
		/// Removes the specified element from the immutable stack and returns the stack after the removal.
		/// </summary>
		/// <returns>Immutable stack with the top element removed</returns>
		public IImmutableStack<TValue> Pop()
			=> new SerializableImmutableStack<TValue>(Store.Pop());


		/// <summary>
		/// Returns the object at the top of the stack without removing it.
		/// </summary>
		/// <returns>Object at the top of the stack</returns>
		public TValue Peek() => Store.Peek();


		/// <summary>
		/// Gets a value that indicates whether this instance of the immutable stack is empty.
		/// </summary>
		public bool IsEmpty => Store.IsEmpty;


		/// <summary>
		/// Populates a SerializationInfo with the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">Serialization info</param>
		/// <param name="context">Serialization context - unused</param>
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			Code.ExpectsArgument(info, nameof(info), TaggingUtilities.ReserveTag(0));

			info.AddValue(SerializedFieldName, new Stack<TValue>(m_store), typeof(Stack<TValue>));
		}


		/// <summary>
		/// Runs when the entire object graph has been deserialized.
		/// </summary>
		/// <param name="sender">Unused</param>
		public void OnDeserialization(object sender)
		{
			Stack<TValue> deserializedValues =
				(Stack<TValue>)m_serializationInfo.GetValue(SerializedFieldName, typeof(Stack<TValue>));

			m_store = ImmutableStack.CreateRange(deserializedValues);
			m_serializationInfo = null;
		}


		private const string SerializedFieldName = "Store";
	}
}