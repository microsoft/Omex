// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#region Using directives

using System;
using System.Collections.Concurrent;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

#endregion

namespace Microsoft.Omex.System.Caching
{
	/// <summary>
	/// The local cache, which ensures only a single copy of the registered objects exists.
	/// </summary>
	public class LocalCache : ICache
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LocalCache"/> class.
		/// </summary>
		public LocalCache() => Cache = new ConcurrentDictionary<Type, object>();


		/// <summary>
		/// Gets a cache entry.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>The object value.</returns>
		public object Get(Type type)
		{
			if (!Code.ValidateArgument(type, nameof(type), TaggingUtilities.ReserveTag(0x23821017 /* tag_967ax */)))
			{
				return null;
			}

			if (!Cache.TryGetValue(type, out object value))
			{
				ULSLogging.LogTraceTag(0x23821018 /* tag_967ay */, Categories.Common, Levels.Verbose,
					"Could not get type '{0}' from cache.", type);
				return null;
			}

			return value;
		}


		/// <summary>
		/// Gets or adds a new cache entry.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="value">The value to construct.</param>
		/// <param name="wasAdded"><c>true</c> if the entry was added to the cache; <c>false</c> otherwise.</param>
		/// <returns>The entry value.</returns>
		public object GetOrAdd(Type type, Func<object> value, out bool wasAdded)
		{
			wasAdded = false;
			if (!Code.ValidateArgument(type, nameof(type), TaggingUtilities.ReserveTag(0x23821019 /* tag_967az */)) ||
				!Code.ValidateArgument(value, nameof(value), TaggingUtilities.ReserveTag(0x2382101a /* tag_967a0 */)))
			{
				return null;
			}

			bool wasAddedInternal = false;
			object attemptedToAdd = null;
			object result = Cache.GetOrAdd(type, currentType =>
				{
					ULSLogging.LogTraceTag(0x2382101b /* tag_967a1 */, Categories.Common, Levels.Verbose,
						"Attempting to add type '{0}' to cache.", currentType);
					wasAddedInternal = true;
					attemptedToAdd = value();
					return attemptedToAdd;
				});

			if (wasAddedInternal)
			{
				if (ReferenceEquals(result, attemptedToAdd))
				{
					ULSLogging.LogTraceTag(0x2382101c /* tag_967a2 */, Categories.Common, Levels.Verbose,
						"Type '{0}' was successfully added to cache.", type);
					wasAdded = true;
				}
				else
				{
					ULSLogging.LogTraceTag(0x2382101d /* tag_967a3 */, Categories.Common, Levels.Verbose,
						"Type '{0}' was not successfully added to cache due to a benign multithreading collision.", type);
				}
			}

			return result;
		}


		/// <summary>
		/// Gets or updates a cache entry.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="value">The value to construct.</param>
		/// <param name="wasUpdated"><c>true</c> if the entry was updated in the cache; <c>false</c> otherwise.</param>
		/// <returns>The entry value.</returns>
		public object AddOrUpdate(Type type, Func<object> value, out bool wasUpdated)
		{
			wasUpdated = false;
			if (!Code.ValidateArgument(type, nameof(type), TaggingUtilities.ReserveTag(0x2382101e /* tag_967a4 */)) ||
				!Code.ValidateArgument(value, nameof(value), TaggingUtilities.ReserveTag(0x2382101f /* tag_967a5 */)))
			{
				return null;
			}

			bool wasUpdatedInternal = false;
			object attemptedToUpdate = null;
			object result = Cache.AddOrUpdate(type, currentType =>
				{
					ULSLogging.LogTraceTag(0x23821020 /* tag_967a6 */, Categories.Common, Levels.Verbose,
						"Type '{0}' does not currently exist in cache. Attempting to add.", type);
					wasUpdatedInternal = true;
					attemptedToUpdate = value();
					return attemptedToUpdate;
				},
				(currentType, oldValue) =>
				{
					ULSLogging.LogTraceTag(0x23821021 /* tag_967a7 */, Categories.Common, Levels.Verbose,
						"Attempting to update type '{0}' in cache.", currentType);
					wasUpdatedInternal = true;
					attemptedToUpdate = value();
					return attemptedToUpdate;
				});

			if (wasUpdatedInternal)
			{
				if (ReferenceEquals(result, attemptedToUpdate))
				{
					ULSLogging.LogTraceTag(0x23821022 /* tag_967a8 */, Categories.Common, Levels.Verbose,
						"Type '{0}' was successfully updated in cache.", type);
					wasUpdated = true;
				}
				else
				{
					ULSLogging.LogTraceTag(0x23821023 /* tag_967a9 */, Categories.Common, Levels.Verbose,
						"Type '{0}' was not successfully updated in cache due to a benign multithreading collision.", type);
				}
			}

			return result;
		}


		/// <summary>
		/// Gets or sets the cache.
		/// </summary>
		private ConcurrentDictionary<Type, object> Cache { get; }
	}
}
