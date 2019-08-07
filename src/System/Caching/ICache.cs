// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.System.Caching
{
	/// <summary>
	/// A cache, which ensures only a single copy of the registered objects exists.
	/// </summary>
	public interface ICache
	{
		/// <summary>
		/// Gets a cache entry.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>The object value.</returns>
		object Get(Type type);


		/// <summary>
		/// Gets or adds a new cache entry.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="value">The value to construct.</param>
		/// <param name="wasAdded"><c>true</c> if the entry was added to the cache; <c>false</c> otherwise.</param>
		/// <returns>The entry value.</returns>
		object GetOrAdd(Type type, Func<object> value, out bool wasAdded);


		/// <summary>
		/// Gets or updates a cache entry.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="value">The value to construct.</param>
		/// <param name="wasUpdated"><c>true</c> if the entry was updated in the cache; <c>false</c> otherwise.</param>
		/// <returns>The entry value.</returns>
		object AddOrUpdate(Type type, Func<object> value, out bool wasUpdated);
	}
}