// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.ObjectPool;
using ServiceFabricHealth = System.Fabric.Health;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal abstract class HealthCheckPublisher : IHealthCheckPublisher
	{
		protected readonly ObjectPool<StringBuilder> StringBuilderPool;

		protected string HealthReportSourceId { get { return HealthReportSourceIdImpl; } }

		protected abstract string HealthReportSourceIdImpl { get; }

		protected const string HealthReportSummaryProperty = "HealthReportSummary";

		public HealthCheckPublisher(ObjectPoolProvider objectPoolProvider) => StringBuilderPool = objectPoolProvider.CreateStringBuilderPool();

		public abstract Task PublishAsync(HealthReport report, CancellationToken cancellationToken);

		protected ServiceFabricHealth.HealthInformation BuildSfHealthInformation(HealthReport report)
		{
			int entriesCount = report.Entries.Count;
			int healthyEntries = 0;
			int degradedEntries = 0;
			int unhealthyEntries = 0;
			foreach (KeyValuePair<string, HealthReportEntry> entryPair in report.Entries)
			{
				switch (entryPair.Value.Status)
				{
					case HealthStatus.Healthy:
						healthyEntries++;
						break;
					case HealthStatus.Degraded:
						degradedEntries++;
						break;
					case HealthStatus.Unhealthy:
					default:
						unhealthyEntries++;
						break;
				}
			}

			StringBuilder descriptionBuilder = StringBuilderPool.Get();

			descriptionBuilder
				.AppendFormat("Health checks executed: {0}. ", entriesCount)
				.AppendFormat("Healthy: {0}/{1}. ", healthyEntries, entriesCount)
				.AppendFormat("Degraded: {0}/{1}. ", degradedEntries, entriesCount)
				.AppendFormat("Unhealthy: {0}/{1}.", unhealthyEntries, entriesCount)
				.AppendLine()
				.AppendFormat("Total duration: {0}.", report.TotalDuration)
				.AppendLine();

			string description = descriptionBuilder.ToString();

			return new ServiceFabricHealth.HealthInformation(HealthReportSourceId, HealthReportSummaryProperty, ToSfHealthState(report.Status))
			{
				Description = description,
			};
		}

		protected ServiceFabricHealth.HealthState ToSfHealthState(HealthStatus healthStatus) =>
			healthStatus switch
			{
				HealthStatus.Healthy => ServiceFabricHealth.HealthState.Ok,
				HealthStatus.Degraded => ServiceFabricHealth.HealthState.Warning,
				HealthStatus.Unhealthy => ServiceFabricHealth.HealthState.Error,
				_ => throw new ArgumentException($"'{healthStatus}' is not a valid health status."),
			};

		protected ServiceFabricHealth.HealthInformation BuildSfHealthInformation(string healthCheckName, HealthReportEntry reportEntry)
		{
			StringBuilder descriptionBuilder = StringBuilderPool.Get();

			if (!string.IsNullOrWhiteSpace(reportEntry.Description))
			{
				descriptionBuilder.Append("Description: ").Append(reportEntry.Description).AppendLine();
			}
			descriptionBuilder.Append("Duration: ").Append(reportEntry.Duration).Append('.').AppendLine();
			if (reportEntry.Exception != null)
			{
				descriptionBuilder.Append("Exception: ").Append(reportEntry.Exception).AppendLine();
			}

			string description = descriptionBuilder.ToString();

			return new ServiceFabricHealth.HealthInformation(HealthReportSourceId, healthCheckName, ToSfHealthState(reportEntry.Status))
			{
				Description = description,
			};
		}
	}
}
