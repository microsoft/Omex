// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Abstractions.Option
{
	/// <summary>
	/// Monitoring option
	/// </summary>
	public class MonitoringOption
	{
		/// <summary>
		/// Path to the setting
		/// </summary>
		public static string MonitoringPath = "Monitoring";

		/// <summary>
		/// Setting to determine whether using Histogram ar Counter for metrics
		/// </summary>
		public bool UseHistogramForActivityMonitoring { get; set; }
	}
}
