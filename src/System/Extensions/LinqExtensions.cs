// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Extensions
{
	/// <summary>
	/// Extensions class for LINQ methods
	/// </summary>
	public static class LinqExtensions
	{
		/// <summary>
		/// Find an argument which maximizes a value calculated using a passed function
		/// </summary>
		/// <typeparam name="TKey">Type of an element in enumerable</typeparam>
		/// <typeparam name="TValue">Type of a value produced by the map function</typeparam>
		/// <param name="enumerable">Enumerable containing keys</param>
		/// <param name="map">Function returning a value of type TValue used to compare keys</param>
		/// <param name="comparer">Comparer, by default is set to the default comparer for TValue</param>
		/// <returns>Argument characterized by the max value</returns>
		public static TKey ArgMax<TKey, TValue>(this IEnumerable<TKey> enumerable, Func<TKey, TValue> map, IComparer<TValue> comparer = null)
		{
			Code.ExpectsArgument(enumerable, nameof(enumerable), TaggingUtilities.ReserveTag(0x2382088e /* tag_9668o */));
			Code.ExpectsArgument(map, nameof(map), TaggingUtilities.ReserveTag(0x2382088f /* tag_9668p */));

			if (comparer == null)
			{
				comparer = Comparer<TValue>.Default;
			}

			using (IEnumerator<TKey> iterator = enumerable.GetEnumerator())
			{
				if (!iterator.MoveNext())
				{
					// enumerable is empty
					throw new ArgumentException("Enumerable should not be empty.");
				}

				TKey bestElementKey = iterator.Current;
				TValue bestElementValue = map(bestElementKey);

				while (iterator.MoveNext())
				{
					TKey currentElementKey = iterator.Current;
					TValue currentElementValue = map(currentElementKey);

					if (comparer.Compare(currentElementValue, bestElementValue) > 0)
					{
						bestElementKey = currentElementKey;
						bestElementValue = currentElementValue;
					}
				}

				return bestElementKey;
			}
		}

		/// <summary>
		/// Find an argument which minimizes a value calculated using a passed function
		/// </summary>
		/// <typeparam name="TKey">Type of an element in enumerable</typeparam>
		/// <typeparam name="TValue">Type of a value produced by the map function</typeparam>
		/// <param name="enumerable">Enumerable containing keys</param>
		/// <param name="map">Function returning a value of type TValue used to compare keys</param>
		/// <param name="comparer">Comparer, by default is set to the default comparer for TValue</param>
		/// <returns>Argument characterized by the min value</returns>
		public static TKey ArgMin<TKey, TValue>(this IEnumerable<TKey> enumerable, Func<TKey, TValue> map, IComparer<TValue> comparer = null)
		{
			Code.ExpectsArgument(enumerable, nameof(enumerable), TaggingUtilities.ReserveTag(0x23820890 /* tag_9668q */));
			Code.ExpectsArgument(map, nameof(map), TaggingUtilities.ReserveTag(0x23820891 /* tag_9668r */));

			if (comparer == null)
			{
				comparer = Comparer<TValue>.Default;
			}

			return enumerable.ArgMax(map, new ReverseComparer<TValue>(comparer));
		}

		private sealed class ReverseComparer<T> : IComparer<T>
		{
			private IComparer<T> Comparer { get; }

			public ReverseComparer(IComparer<T> comparer)
			{
				Comparer = Code.ExpectsObject(comparer, nameof(comparer), TaggingUtilities.ReserveTag(0x23820892 /* tag_9668s */));
			}

			public int Compare(T x, T y)
			{
				return Comparer.Compare(y, x);
			}
		}
	}
}
