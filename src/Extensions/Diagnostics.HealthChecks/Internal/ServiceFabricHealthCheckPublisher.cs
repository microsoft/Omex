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
		private const string SourceId = nameof(ServiceFabricHealthCheckPublisher);

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
				m_logger.LogWarning(Tag.Create(), "Publisher run before partition provided");

				return Task.CompletedTask;
			}

			partition.ReportPartitionHealth(new SfHealthInformation(SourceId, "Summary", ConvertStatus(report.Status)));

			foreach(KeyValuePair<string, HealthReportEntry> entryPair in report.Entries)
			{
				HealthReportEntry entry = entryPair.Value;
				SfHealthInformation sfHealthInformation = new SfHealthInformation(SourceId, entryPair.Key, ConvertStatus(entry.Status))
				{
					Description = CreateDescription(entry)
				};

				try
				{
					partition.ReportPartitionHealth(sfHealthInformation);
				}
				catch (Exception exception)
				{
					m_logger.LogError(Tag.Create(), exception, "Failed to report partition health");
				}
			}

			return Task.CompletedTask;
		}

		private string CreateDescription(HealthReportEntry entry)
		{
			// Healthy reports won't be displayed and most of the checks will be healthy,
			// so we are creating a detailed description only for failed checks and avoiding allocations for healthy
			if (entry.Status == HealthStatus.Healthy)
			{
				return entry.Description;
			}

			return new StringBuilder()
				.Append("Exception:").AppendLine(entry.Exception?.ToString())
				.Append("Description:").AppendLine(entry.Description)
				.Append("Duration:").AppendLine(entry.Duration.ToString())
				.Append("Tags:").AppendLine(string.Join(",", entry.Tags))
				.ToString();
		}

		private SfHealthState ConvertStatus(HealthStatus state) =>
			state switch
			{
				HealthStatus.Healthy => SfHealthState.Ok,
				HealthStatus.Degraded => SfHealthState.Warning,
				HealthStatus.Unhealthy => SfHealthState.Error,
				_ => SfHealthState.Invalid
			};
	}
}
