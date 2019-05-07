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
			if (!Code.ValidateArgument(type, nameof(type), TaggingUtilities.ReserveTag(0x238217c1 /* tag_9675b */)))
			{
				return null;
			}

			if (!Cache.TryGetValue(type, out object value))
			{
				ULSLogging.LogTraceTag(0x238217c2 /* tag_9675c */, Categories.Common, Levels.Verbose,
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
			if (!Code.ValidateArgument(type, nameof(type), TaggingUtilities.ReserveTag(0x238217c3 /* tag_9675d */)) ||
				!Code.ValidateArgument(value, nameof(value), TaggingUtilities.ReserveTag(0x238217c4 /* tag_9675e */)))
			{
				return null;
			}

			bool wasAddedInternal = false;
			object attemptedToAdd = null;
			object result = Cache.GetOrAdd(type, currentType =>
				{
					ULSLogging.LogTraceTag(0x238217c5 /* tag_9675f */, Categories.Common, Levels.Verbose,
						"Attempting to add type '{0}' to cache.", currentType);
					wasAddedInternal = true;
					attemptedToAdd = value();
					return attemptedToAdd;
				});

			if (wasAddedInternal)
			{
				if (ReferenceEquals(result, attemptedToAdd))
				{
					ULSLogging.LogTraceTag(0x238217c6 /* tag_9675g */, Categories.Common, Levels.Verbose,
						"Type '{0}' was successfully added to cache.", type);
					wasAdded = true;
				}
				else
				{
					ULSLogging.LogTraceTag(0x238217c7 /* tag_9675h */, Categories.Common, Levels.Verbose,
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
			if (!Code.ValidateArgument(type, nameof(type), TaggingUtilities.ReserveTag(0x238217c8 /* tag_9675i */)) ||
				!Code.ValidateArgument(value, nameof(value), TaggingUtilities.ReserveTag(0x238217c9 /* tag_9675j */)))
			{
				return null;
			}

			bool wasUpdatedInternal = false;
			object attemptedToUpdate = null;
			object result = Cache.AddOrUpdate(type, currentType =>
				{
					ULSLogging.LogTraceTag(0x238217ca /* tag_9675k */, Categories.Common, Levels.Verbose,
						"Type '{0}' does not currently exist in cache. Attempting to add.", type);
					wasUpdatedInternal = true;
					attemptedToUpdate = value();
					return attemptedToUpdate;
				},
				(currentType, oldValue) =>
				{
					ULSLogging.LogTraceTag(0x238217cb /* tag_9675l */, Categories.Common, Levels.Verbose,
						"Attempting to update type '{0}' in cache.", currentType);
					wasUpdatedInternal = true;
					attemptedToUpdate = value();
					return attemptedToUpdate;
				});

			if (wasUpdatedInternal)
			{
				if (ReferenceEquals(result, attemptedToUpdate))
				{
					ULSLogging.LogTraceTag(0x238217cc /* tag_9675m */, Categories.Common, Levels.Verbose,
						"Type '{0}' was successfully updated in cache.", type);
					wasUpdated = true;
				}
				else
				{
					ULSLogging.LogTraceTag(0x238217cd /* tag_9675n */, Categories.Common, Levels.Verbose,
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
