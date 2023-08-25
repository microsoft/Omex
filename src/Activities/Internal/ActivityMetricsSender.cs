﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
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

		public ActivityMetricsSender(IExecutionContext executionContext, IHostEnvironment hostEnvironment, ICustomBaggageDimensions customBaggageDimensions, ICustomTagObjectsDimensions customTagObjectsDimensions)
		{
			m_context = executionContext;
			m_hostEnvironment = hostEnvironment;
			m_meter = new Meter("Microsoft.Omex.Activities", "1.0.0");
			m_activityHistogram = m_meter.CreateHistogram<long>("Activities");
			m_healthCheckActivityHistogram = m_meter.CreateHistogram<long>("HealthCheckActivities");
			m_customBaggageDimension = customBaggageDimensions.CustomDimensions;
			m_customTagObjectsDimension = customTagObjectsDimensions.CustomDimensions;
		}

		public void SendActivityMetric(Activity activity)
		{
			Histogram<long> histogram = activity.IsHealthCheck() ? m_healthCheckActivityHistogram : m_activityHistogram;

			long durationMs = Convert.ToInt64(activity.Duration.TotalMilliseconds);

			TagList tagList = new()
				{
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
					tagList.Add(dimension, baggageItem);
				}
			}

			foreach (string dimension in m_customTagObjectsDimension)
			{
				object? tagItem = activity.GetTagItem(dimension);
				if (tagItem != null)
				{
					tagList.Add(dimension, tagItem);
				}
			}

			histogram.Record(durationMs, tagList);
		}

		public void Dispose() => m_meter.Dispose();
	}
}
