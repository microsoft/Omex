// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.Omex.Extensions.Abstractions.Option;

namespace Microsoft.Omex.Extensions.Activities
{
	internal sealed class ActivityMetricsSender : IActivitiesEventSender, IDisposable
	{
		private readonly Meter m_meter;
		private readonly Counter<double> m_activityCounter;
		private readonly Counter<double> m_healthCheckActivityCounter;
		private readonly Histogram<double> m_activityHistogram;
		private readonly Histogram<double> m_healthCheckActivityHistogram;
		private readonly bool m_useHistogramForActivity;
		private readonly IExecutionContext m_context;
		private readonly IHostEnvironment m_hostEnvironment;
		private readonly ArrayPool<KeyValuePair<string, object?>> m_arrayPool;

		public ActivityMetricsSender(IExecutionContext executionContext, IHostEnvironment hostEnvironment, IOptions<MonitoringOption> monitoringOption)
		{
			m_context = executionContext;
			m_hostEnvironment = hostEnvironment;
			m_meter = new Meter("Microsoft.Omex.Activities", "1.0.0");
			m_activityCounter = m_meter.CreateCounter<double>("Activities");
			m_healthCheckActivityCounter = m_meter.CreateCounter<double>("HealthCheckActivities");
			m_activityHistogram = m_meter.CreateHistogram<double>("Activities");
			m_healthCheckActivityHistogram = m_meter.CreateHistogram<double>("HealthCheckActivities");
			m_useHistogramForActivity = monitoringOption.Value.UseHistogramForActivityMonitoring;

			m_arrayPool = ArrayPool<KeyValuePair<string, object?>>.Create();
		}

		public void SendActivityMetric(Activity activity)
		{
			double durationMs = activity.Duration.TotalMilliseconds;

			int tagsCount = s_customTags.Length + activity.TagObjects.Count() + activity.Baggage.Count();

			KeyValuePair<string, object?>[] tags = m_arrayPool.Rent(tagsCount);

			int index = 0;

			foreach (Func<ActivityMetricsSender, Activity, KeyValuePair<string, object?>> getter in s_customTags)
			{
				tags[index++] = getter(this, activity);
			}

			foreach (KeyValuePair<string, string?> baggage in activity.Baggage)
			{
				tags[index++] = CreatePair(baggage.Key, baggage.Value);
			}

			foreach (KeyValuePair<string, object?> tag in activity.TagObjects)
			{
				tags[index++] = tag;
			}

			ReadOnlySpan<KeyValuePair<string, object?>> tagsSpan = MemoryExtensions.AsSpan(tags, 0, tagsCount);

			Histogram<double> histogram = m_activityHistogram;
			Counter<double> counter = m_activityCounter;
			if (activity.IsHealthCheck())
			{
				histogram = m_healthCheckActivityHistogram;
				counter = m_healthCheckActivityCounter;
			}

			if (m_useHistogramForActivity)
			{
				histogram.Record(durationMs, tagsSpan);
			}
			else
			{
				counter.Add(durationMs, tagsSpan);
			}

			m_arrayPool.Return(tags, clearArray: true);
		}

		public void Dispose() => m_meter.Dispose();

		private static readonly Func<ActivityMetricsSender, Activity, KeyValuePair<string, object?>>[] s_customTags = new Func<ActivityMetricsSender, Activity, KeyValuePair<string, object?>>[]
		{
			static (sender, activity) => CreatePair("Name", activity.OperationName),
			static (sender, activity) => CreatePair("Environment", sender.m_hostEnvironment.EnvironmentName),
			static (sender, activity) => CreatePair("RegionName", sender.m_context.RegionName),
			static (sender, activity) => CreatePair("Cluster", sender.m_context.Cluster),
			static (sender, activity) => CreatePair("ApplicationName", sender.m_context.ApplicationName),
			static (sender, activity) => CreatePair("ServiceName", sender.m_context.ServiceName),
			static (sender, activity) => CreatePair("BuildVersion", sender.m_context.BuildVersion),
			static (sender, activity) => CreatePair("NodeName", sender.m_context.NodeName),
			static (sender, activity) => CreatePair("MachineId", sender.m_context.MachineId),
			static (sender, activity) => CreatePair("DeploymentSlice", sender.m_context.DeploymentSlice),
			static (sender, activity) => CreatePair("IsCanary", sender.m_context.IsCanary),
			static (sender, activity) => CreatePair("IsPrivateDeployment", sender.m_context.IsPrivateDeployment)
		};

		internal const string UseHistogramForActivityMonitoringConfigurationPath = "Monitoring:UseHistogramForActivityMonitoring";

		private static KeyValuePair<string, object?> CreatePair(string key, object? value) => new(key, value);
	}
}
