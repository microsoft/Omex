// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;

namespace Microsoft.Omex.Extensions.Activities
{
	internal sealed class ActivityMetricsSender : IActivitiesEventSender, IDisposable
	{
		private readonly Meter m_meter;
		private readonly Counter<double> m_activityCounter;
		private readonly Counter<double> m_healthCheckActivityCounter;
		private readonly IExecutionContext m_context;
		private readonly IHostEnvironment m_hostEnvironment;
		private readonly ArrayPool<KeyValuePair<string, object?>> m_arrayPool;

		public ActivityMetricsSender(IExecutionContext executionContext, IHostEnvironment hostEnvironment)
		{
			m_context = executionContext;
			m_hostEnvironment = hostEnvironment;
			m_meter = new Meter("Microsoft.Omex.Activities", "1.0.0");
			m_activityCounter = m_meter.CreateCounter<double>("Activities");
			m_healthCheckActivityCounter = m_meter.CreateCounter<double>("HealthCheckActivities");
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
				tags[index++] = new KeyValuePair<string, object?>(baggage.Key, baggage.Value);
			}

			foreach (KeyValuePair<string, object?> tag in activity.TagObjects)
			{
				tags[index++] = tag;
			}

			ReadOnlySpan<KeyValuePair<string, object?>> tagsSpan = MemoryExtensions.AsSpan(tags, 0, tagsCount);

			if (activity.IsHealthCheck())
			{
				m_healthCheckActivityCounter.Add(durationMs, tagsSpan);
			}
			else
			{
				m_activityCounter.Add(durationMs, tagsSpan);
			}

			m_arrayPool.Return(tags, clearArray: true);
		}

		public void Dispose() => m_meter.Dispose();

		private static readonly Func<ActivityMetricsSender, Activity, KeyValuePair<string, object?>>[] s_customTags = new Func<ActivityMetricsSender, Activity, KeyValuePair<string, object?>>[]
		{
			static (sender, activity) => CreatePair("ActivityId", activity.Id),
			static (sender, activity) => CreatePair("TraceId", activity.TraceId),
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

		private static KeyValuePair<string, object?> CreatePair(string key, object? value) => new(key, value);
	}
}
