// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Activities
{
	/// <summary>
	/// Type created to temporary combine ActivityEventSender and ActivityMetricsSender, it should be deleted after removal for ActivityEventSender
	/// </summary>
	[Obsolete($"{nameof(AggregatedActivitiesEventSender)} is obsolete and pending for removal by 1 July 2024. On the removal, only {nameof(ActivityMetricsSender)} is available to send metrics by OpenTelemetry. Code: 8913598")]
	internal sealed class AggregatedActivitiesEventSender : IActivitiesEventSender
	{
		private readonly ActivityEventSender m_etwSender;
		private readonly ActivityMetricsSender m_metricsSender;

		public AggregatedActivitiesEventSender(ActivityEventSender etwSender, ActivityMetricsSender metricsSender)
		{
			m_etwSender = etwSender;
			m_metricsSender = metricsSender;
		}

		public void SendActivityMetric(Activity activity)
		{
			m_etwSender.SendActivityMetric(activity);
			m_metricsSender.SendActivityMetric(activity);
		}
	}
}
