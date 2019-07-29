// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Microsoft.Omex.System.Validation;
using Microsoft.Omex.System.Logging;

namespace Microsoft.Omex.System.AspNetCore
{
	/// <summary>
	/// Serializable implementation of the ImmutableDictionary
	/// </summary>
	/// <typeparam name="TKey">Key type</typeparam>
	/// <typeparam name="TValue">Value type</typeparam>
	[Serializable]
	public class SerializableImmutableDictionary<TKey, TValue> : MarshalByRefObject, IImmutableDictionary<TKey, TValue>, ISerializable, IDeserializationCallback
	{
		[XmlIgnore]
		[NonSerialized]
		private ImmutableDictionary<TKey, TValue> m_store;


		/// <summary>
		/// Stores serilization info received in the constructor until
		/// the deserilization callback is called and the object can be deserilized
		/// </summary>
		[XmlIgnore]
		[NonSerialized]
		private SerializationInfo m_serializationInfo;


		private ImmutableDictionary<TKey, TValue> Store => m_store;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="store">Data store</param>
		public SerializableImmutableDictionary(ImmutableDictionary<TKey, TValue> store)
		{
			Code.ExpectsArgument(store, nameof(store), TaggingUtilities.ReserveTag(0x238506ce /* tag_97q1o */));

			m_store = store;
		}


		/// <summary>
		/// Constructor
		/// </summary>
		public SerializableImmutableDictionary()
		{
			m_store = ImmutableDictionary<TKey, TValue>.Empty;
		}


		/// <summary>
		/// Constructor used for deserialization.
		/// </summary>
		/// <param name="info">Serialization info</param>
		/// <param name="context">Serialization context - unused</param>
		/// <remarks>Stores serialization info in a field to allow deserialization callback to complete deserialization</remarks>
		protected SerializableImmutableDictionary(SerializationInfo info, StreamingContext context)
		{
			Code.ExpectsArgument(info, nameof(info), TaggingUtilities.ReserveTag(0x238506cf /* tag_97q1p */));

			m_serializationInfo = info;
		}


		/// <summary>
		/// Returns an enumerator that iterates through the immutable dictionary.
		/// </summary>
		/// <returns>Enumerator</returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Store.GetEnumerator();


		/// <summary>
		/// Returns an enumerator that iterates through the immutable dictionary.
		/// </summary>
		/// <returns>Enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


		/// <summary>
		/// Gets the number of key/value pairs in the immutable dictionary.
		/// </summary>
		/// <returns>Number of key/value pairs in the immutable dictionary</returns>
		public int Count => Store.Count;


		/// <summary>
		/// Determines whether this immutable dictionary contains the specified key/value pair.
		/// </summary>
		/// <param name="key">Key</param>
		/// <returns>True if dictionary contains the key</returns>
		public bool ContainsKey(TKey key) => Store.ContainsKey(key);


		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">Key</param>
		/// <param name="value">Value</param>
		/// <returns>The value associated with the specified key</returns>
		public bool TryGetValue(TKey key, out TValue value) => Store.TryGetValue(key, out value);


		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">Key</param>
		/// <returns>Value associated with the specified key</returns>
		public TValue this[TKey key] => Store[key];


		/// <summary>
		/// Gets the keys in the immutable dictionary.
		/// </summary>
		public IEnumerable<TKey> Keys => Store.Keys;


		/// <summary>
		/// Gets the values in the immutable dictionary.
		/// </summary>
		public IEnumerable<TValue> Values => Store.Values;


		/// <summary>
		/// Retrieves an empty immutable dictionary that has the same ordering and key/value comparison rules as this dictionary instance.
		/// </summary>
		/// <returns></returns>
		public IImmutableDictionary<TKey, TValue> Clear()
			=> new SerializableImmutableDictionary<TKey, TValue>(Store.Clear());


		/// <summary>
		/// Adds an element with the specified key and value to the immutable dictionary.
		/// </summary>
		/// <param name="key">Key</param>
		/// <param name="value">Value</param>
		/// <returns>Immutable dictionary with the added value</returns>
		public IImmutableDictionary<TKey, TValue> Add(TKey key, TValue value)
			=> new SerializableImmutableDictionary<TKey, TValue>(Store.Add(key, value));


		/// <summary>
		/// Adds the specified key/value pairs to the immutable dictionary.
		/// </summary>
		/// <param name="pairs">Key/Value pairs</param>
		/// <returns>Immutable dictionary with the pairs added</returns>
		public IImmutableDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
			=> new SerializableImmutableDictionary<TKey, TValue>(Store.AddRange(pairs));


		/// <summary>
		/// Sets the specified key and value in the immutable dictionary, possibly overwriting an existing value for the key.
		/// </summary>
		/// <param name="key">Key</param>
		/// <param name="value">Value</param>
		/// <returns>Immutable dictionary with the item set</returns>
		public IImmutableDictionary<TKey, TValue> SetItem(TKey key, TValue value)
			=> new SerializableImmutableDictionary<TKey, TValue>(Store.SetItem(key, value));


		/// <summary>
		/// Sets the specified key/value pairs in the immutable dictionary, possibly overwriting existing values for the keys.
		/// </summary>
		/// <param name="items">Items to set</param>
		/// <returns>Immutable dictionary with those items set</returns>
		public IImmutableDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
			=> new SerializableImmutableDictionary<TKey, TValue>(Store.SetItems(items));


		/// <summary>
		/// Removes the elements with the specified keys from the immutable dictionary.
		/// </summary>
		/// <param name="keys">Keys</param>
		/// <returns>Immutable dictionary with the keys removed</returns>
		public IImmutableDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
			=> new SerializableImmutableDictionary<TKey, TValue>(Store.RemoveRange(keys));


		/// <summary>
		/// Removes the element with the specified key from the immutable dictionary.
		/// </summary>
		/// <param name="key">Key</param>
		/// <returns>Immutable dictionary with the key removed</returns>
		public IImmutableDictionary<TKey, TValue> Remove(TKey key)
			=> new SerializableImmutableDictionary<TKey, TValue>(Store.Remove(key));


		/// <summary>
		/// Determines whether this immutable dictionary contains the specified key/value pair.
		/// </summary>
		/// <param name="pair">Pair to check</param>
		/// <returns>True if dictionary contains the pair</returns>
		public bool Contains(KeyValuePair<TKey, TValue> pair) => Store.Contains(pair);


		/// <summary>
		/// Determines whether this dictionary contains a specified key.
		/// </summary>
		/// <param name="equalKey">Key</param>
		/// <param name="actualKey">Value</param>
		/// <returns>True if the key is in the dictionary</returns>
		public bool TryGetKey(TKey equalKey, out TKey actualKey) => Store.TryGetKey(equalKey, out actualKey);


		/// <summary>
		/// Populates a SerializationInfo with the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">Serialization info</param>
		/// <param name="context">Serialization context - unused</param>
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			Code.ExpectsArgument(info, nameof(info), TaggingUtilities.ReserveTag(0x238506d0 /* tag_97q1q */));

			info.AddValue(SerializedFieldName, m_store.ToArray(), typeof(KeyValuePair<TKey, TValue>[]));
			info.AddValue(KeyComparerFieldName, m_store.KeyComparer, typeof(IEqualityComparer<TKey>));
			info.AddValue(ValueComparerFieldName, m_store.ValueComparer, typeof(IEqualityComparer<TValue>));
		}


		/// <summary>
		/// Runs when the entire object graph has been deserialized.
		/// </summary>
		/// <param name="sender">Unused</param>
		public void OnDeserialization(object sender)
		{
			KeyValuePair<TKey, TValue>[] deserializedValues =
				(KeyValuePair<TKey, TValue>[])m_serializationInfo.GetValue(SerializedFieldName, typeof(KeyValuePair<TKey, TValue>[]));

			IEqualityComparer<TKey> keyComparer =
					(IEqualityComparer<TKey>)m_serializationInfo.GetValue(KeyComparerFieldName, typeof(IEqualityComparer<TKey>));

			IEqualityComparer<TValue> valueComparer =
					(IEqualityComparer<TValue>)m_serializationInfo.GetValue(ValueComparerFieldName, typeof(IEqualityComparer<TValue>));

			m_store = deserializedValues.ToImmutableDictionary().WithComparers(keyComparer, valueComparer);

			m_serializationInfo = null;
		}


		private const string SerializedFieldName = "Store";


		private const string KeyComparerFieldName = "KeyComparer";


		private const string ValueComparerFieldName = "ValueComparer";
	}
}
