// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Activities
{
	/// <summary>
	/// Monitoring option
	/// </summary>
	internal class ActivityOption
	{
		/// <summary>
		/// Path to the setting
		/// </summary>
		public static string MonitoringPath = "Monitoring";

		/// <summary>
		/// Disable ActivityEventSender so Activity metric is only sent via ActivityMetricsSender
		/// </summary>
		public bool ActivityEventSenderEnabled { get; set; } = true;

		/// <summary>
		/// Sets each activities parent name as a dimension value.
		/// </summary>
		public bool SetParentNameAsDimensionEnabled { get; set; } = false;
	}
}
