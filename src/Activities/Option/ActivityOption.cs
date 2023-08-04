﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Activities.Option
{
	/// <summary>
	/// Monitoring option
	/// </summary>
	public class ActivityOption
	{
		/// <summary>
		/// Path to the setting
		/// </summary>
		public static string MonitoringPath = "Monitoring";

		/// <summary>
		/// Disable ActivityEventSender so Activity metric is only sent via ActivityMetricsSender
		/// </summary>
		public bool DisableActivityEventSender { get; set; }
	}
}