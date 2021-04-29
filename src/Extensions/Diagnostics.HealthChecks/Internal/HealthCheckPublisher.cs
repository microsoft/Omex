// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Fabric;
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

		internal abstract string HealthReportSourceId { get; }

		internal const string HealthReportSummaryProperty = "HealthReportSummary";

		public HealthCheckPublisher(ObjectPoolProvider objectPoolProvider) => StringBuilderPool = objectPoolProvider.CreateStringBuilderPool();

		public abstract Task PublishAsync(HealthReport report, CancellationToken cancellationToken);

		private ServiceFabricHealth.HealthInformation FinalizeHealthReport(string healthReportSummaryProperty, ServiceFabricHealth.HealthState healthState)
		{
			return new ServiceFabricHealth.HealthInformation(HealthReportSourceId, healthReportSummaryProperty, healthState);
		}
		protected void PublishAllEntries(HealthReport report, Action<ServiceFabricHealth.HealthInformation> publishFunc,
			 CancellationToken cancellationToken)
		{
			try
			{
				// We trust the framework to ensure that the report is not null and doesn't contain null entries.
				foreach (KeyValuePair<string, HealthReportEntry> entryPair in report.Entries)
				{
					cancellationToken.ThrowIfCancellationRequested();
					publishFunc(BuildSfHealthInformation(entryPair.Key, entryPair.Value));
				}

				cancellationToken.ThrowIfCancellationRequested();
				publishFunc(BuildSfHealthInformation(report));
			}
			catch (FabricObjectClosedException)
			{
				// Ignore, the service instance is closing.
			}
		}


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
			string description;

			try
			{


				descriptionBuilder
					.AppendFormat("Health checks executed: {0}. ", entriesCount)
					.AppendFormat("Healthy: {0}/{1}. ", healthyEntries, entriesCount)
					.AppendFormat("Degraded: {0}/{1}. ", degradedEntries, entriesCount)
					.AppendFormat("Unhealthy: {0}/{1}.", unhealthyEntries, entriesCount)
					.AppendLine()
					.AppendFormat("Total duration: {0}.", report.TotalDuration)
					.AppendLine();

				description = descriptionBuilder.ToString();
			}
			finally
			{
				StringBuilderPool.Return(descriptionBuilder);
			}

			ServiceFabricHealth.HealthInformation healthInfo = FinalizeHealthReport(HealthReportSummaryProperty, ToSfHealthState(report.Status));
			healthInfo.Description = description;
			return healthInfo;
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
			string description;
			try
			{
				if (!string.IsNullOrWhiteSpace(reportEntry.Description))
				{
					descriptionBuilder.Append("Description: ").Append(reportEntry.Description).AppendLine();
				}
				descriptionBuilder.Append("Duration: ").Append(reportEntry.Duration).Append('.').AppendLine();
				if (reportEntry.Exception != null)
				{
					descriptionBuilder.Append("Exception: ").Append(reportEntry.Exception).AppendLine();
				}

				description = descriptionBuilder.ToString();
			}
			finally
			{
				StringBuilderPool.Return(descriptionBuilder);
			}

			ServiceFabricHealth.HealthInformation healthInfo = FinalizeHealthReport(healthCheckName, ToSfHealthState(reportEntry.Status));
			healthInfo.Description = description;
			return healthInfo;
		}
	}
}
