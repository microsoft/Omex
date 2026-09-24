// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;

namespace Microsoft.Omex.Extensions.Activities
{
	internal sealed class ActivityMetricsSender : IActivitiesEventSender, IDisposable
	{
		private readonly Meter m_meter;
		private readonly Histogram<long> m_activityHistogram;
		private readonly Histogram<long> m_healthCheckActivityHistogram;
		private readonly IExecutionContext m_context;
		private readonly IHostEnvironment m_hostEnvironment;
		private readonly HashSet<string> m_customBaggageDimension;
		private readonly HashSet<string> m_customTagObjectsDimension;
		private readonly bool m_isSetParentNameAsDimensionEnabled;

		// Caps metric tag value length to reduce cardinality risk
		// from externally controlled baggage or tag values
		private const int MaxMetricTagValueLength = 256;

		public ActivityMetricsSender(
			IExecutionContext executionContext,
			IHostEnvironment hostEnvironment,
			ICustomBaggageDimensions customBaggageDimensions,
			ICustomTagObjectsDimensions customTagObjectsDimensions,
			IOptions<ActivityOption> activityOptions)
		{
			m_context = executionContext;
			m_hostEnvironment = hostEnvironment;
			m_meter = new Meter(OmexActivityConfiguration.MeterName, "1.0.0");
			m_activityHistogram = m_meter.CreateHistogram<long>(OmexActivityConfiguration.ActivitiesHistogramName);
			m_healthCheckActivityHistogram = m_meter.CreateHistogram<long>(OmexActivityConfiguration.HealthCheckActivitiesHistogramName);
			m_customBaggageDimension = customBaggageDimensions.CustomDimensions;
			m_customTagObjectsDimension = customTagObjectsDimensions.CustomDimensions;
			m_isSetParentNameAsDimensionEnabled = activityOptions.Value.SetParentNameAsDimensionEnabled;
		}

		public void SendActivityMetric(Activity activity)
		{
			Histogram<long> histogram = activity.IsHealthCheck() ? m_healthCheckActivityHistogram : m_activityHistogram;

			long durationMs = Convert.ToInt64(activity.Duration.TotalMilliseconds);

			TagList tagList = new()
				{
					{ "Tenant", $"{m_context.Cluster.ToLowerInvariant()}-{m_hostEnvironment.EnvironmentName.ToLowerInvariant()}-{m_context.DeploymentSlice}" },
					{ "Name", activity.OperationName },
					{ "RegionName", m_context.RegionName },
					{ "ServiceName", m_context.ServiceName },
					{ "BuildVersion", m_context.BuildVersion },
					{ "Environment", m_hostEnvironment.EnvironmentName },
					{ "Cluster", m_context.Cluster },
					{ "ApplicationName", m_context.ApplicationName },
					{ "NodeName", m_context.NodeName },
					{ "MachineId", m_context.MachineId },
					{ "DeploymentSlice", m_context.DeploymentSlice },
					{ "IsCanary", m_context.IsCanary },
					{ "IsPrivateDeployment", m_context.IsPrivateDeployment }
				};

			foreach (string dimension in m_customBaggageDimension)
			{
				string? baggageItem = activity.GetBaggageItem(dimension);
				if (!string.IsNullOrWhiteSpace(baggageItem))
				{
					tagList.Add(dimension, TruncateSafe(baggageItem, MaxMetricTagValueLength));
				}
			}

			foreach (string dimension in m_customTagObjectsDimension)
			{
				object? tagItem = activity.GetTagItem(dimension);
				if (tagItem != null)
				{
					// Only truncate string values — preserve original
					// type for numeric/bool to avoid breaking exporters
					if (tagItem is string stringValue)
					{
						tagList.Add(dimension, TruncateSafe(stringValue, MaxMetricTagValueLength));
					}
					else
					{
						tagList.Add(dimension, tagItem);
					}
				}
			}

			if (m_isSetParentNameAsDimensionEnabled)
			{
				Activity? parent = activity.Parent;
				if (!string.IsNullOrEmpty(parent?.OperationName))
				{
					tagList.Add("ParentName", parent.OperationName);
				}
			}

			histogram.Record(durationMs, tagList);
		}

		// Truncates string to maxLength while avoiding
		// splitting UTF-16 surrogate pairs
		private static string TruncateSafe(string value, int maxLength)
		{
			if (value.Length <= maxLength)
				return value;

			if (char.IsHighSurrogate(value[maxLength - 1]))
				return value[..(maxLength - 1)];

			return value[..maxLength];
		}

		public void Dispose() => m_meter.Dispose();
	}
}
