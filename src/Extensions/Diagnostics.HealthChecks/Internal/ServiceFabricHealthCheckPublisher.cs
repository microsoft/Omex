// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using SfHealthInformation = System.Fabric.Health.HealthInformation;
using SfHealthState = System.Fabric.Health.HealthState;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class ServiceFabricHealthCheckPublisher : IHealthCheckPublisher
	{
		private const string SfHealthInformationSourceId = nameof(ServiceFabricHealthCheckPublisher);

		private readonly IAccessor<IServicePartition> m_partitionAccessor;

		private readonly ILogger<ServiceFabricHealthCheckPublisher> m_logger;

		public ServiceFabricHealthCheckPublisher(
			IAccessor<IServicePartition> partitionAccessor,
			ILogger<ServiceFabricHealthCheckPublisher> logger)
		{
			m_partitionAccessor = partitionAccessor;
			m_logger = logger;
		}

		public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
		{
			IServicePartition? partition = m_partitionAccessor.Value;
			if (partition == null)
			{
				m_logger.LogWarning(Tag.Create(), "Publisher run before partition provided.");

				return Task.CompletedTask;
			}

			// Avoiding repeated pattern matching for each report entry.
			Action<SfHealthInformation> reportHealth = partition switch
			{
				IStatefulServicePartition statefulPartition => statefulPartition.ReportReplicaHealth,
				IStatelessServicePartition statelessPartition => statelessPartition.ReportInstanceHealth,
				_ => (_) => { }, // NOOP, not going to happen, but we need a default case for the switch.
			};

			try
			{
				foreach (KeyValuePair<string, HealthReportEntry> entryPair in report.Entries)
				{
					cancellationToken.ThrowIfCancellationRequested();
					reportHealth(BuildSfHealthInformation(entryPair.Key, entryPair.Value));
				}

				cancellationToken.ThrowIfCancellationRequested();
				reportHealth(BuildSfHealthInformation(report));
			}
			catch (FabricObjectClosedException)
			{
				// Ignore, the service instance is closing.
			}

			return Task.CompletedTask;
		}

		private SfHealthInformation BuildSfHealthInformation(string healthCheckName, HealthReportEntry reportEntry)
		{
			StringBuilder descriptionBuilder = new StringBuilder();
			if (!string.IsNullOrWhiteSpace(reportEntry.Description))
			{
				descriptionBuilder.AppendLine($"Description: {reportEntry.Description}");
			}
			descriptionBuilder.AppendLine($"Duration: {reportEntry.Duration}.");
			if (reportEntry.Exception != null)
			{
				descriptionBuilder.AppendLine($"Exception: {reportEntry.Exception}");
			}

			return new SfHealthInformation(SfHealthInformationSourceId, healthCheckName, ToSfHealthState(reportEntry.Status))
			{
				Description = descriptionBuilder.ToString(),
			};
		}

		private SfHealthInformation BuildSfHealthInformation(HealthReport report)
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

			StringBuilder descriptionBuilder = new StringBuilder()
				.AppendLine($"Health checks executed: {entriesCount}. Healthy: {healthyEntries}/{entriesCount}. Degraded: {degradedEntries}/{entriesCount}. Unhealthy: {unhealthyEntries}/{entriesCount}.")
				.AppendLine($"Total duration: {report.TotalDuration}.");

			return new SfHealthInformation(SfHealthInformationSourceId, "HealthChecksSummary", ToSfHealthState(report.Status))
			{
				Description = descriptionBuilder.ToString(),
			};
		}

		private SfHealthState ToSfHealthState(HealthStatus healthStatus) =>
			healthStatus switch
			{
				HealthStatus.Healthy => SfHealthState.Ok,
				HealthStatus.Degraded => SfHealthState.Warning,
				HealthStatus.Unhealthy => SfHealthState.Error,
				_ => SfHealthState.Error, // Invalid health state can't be reported to SF.
			};
	}
}
