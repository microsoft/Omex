// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	/// <summary>
	/// Additional parameters to configure health check
	/// </summary>
	public class HealthCheckParameters
	{
		/// <summary>
		/// Additional data to add to health check result
		/// </summary>
		public IReadOnlyDictionary<string, object> ReportData { get; }

		/// <summary>
		/// Creates HealthCheckParameters instance
		/// </summary>
		public HealthCheckParameters(KeyValuePair<string, object>[] reportData)
		{
			if (reportData.Length == 0)
			{
				ReportData = s_emptyDictionary;
			}
			else
			{
				// should be replaced by passing enumerable to constructor after we drop full framework support
				Dictionary<string, object> dictionary = new Dictionary<string, object>(reportData.Length);
				foreach (KeyValuePair<string, object> pair in reportData)
				{
					// could be replaced with TryAdd, after we drop full framework support
					try
					{
						dictionary.Add(pair.Key, pair.Value);
					}
					catch (ArgumentNullException exception)
					{
						throw new ArgumentNullException("Null key in HealthCheckParameters", exception);
					}
					catch (ArgumentException exception)
					{
						throw new ArgumentException("Duplicated key in HealthCheckParameters", nameof(reportData), exception);
					}
				}

				ReportData = new ReadOnlyDictionary<string, object>(dictionary);
			}
		}

		private static readonly IReadOnlyDictionary<string, object> s_emptyDictionary =
			new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
	}
}
