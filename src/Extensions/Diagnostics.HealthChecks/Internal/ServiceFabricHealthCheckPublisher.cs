// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Abstractions;
using SfHealthInformation = System.Fabric.Health.HealthInformation;
using SfHealthState = System.Fabric.Health.HealthState;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class ServiceFabricHealthCheckPublisher : IHealthCheckPublisher
	{
		private const string SourceId = nameof(ServiceFabricHealthCheckPublisher);

		private IServicePartition? m_partition;

		public ServiceFabricHealthCheckPublisher(IAccessor<IServicePartition> partitionAccessor)
		{
			partitionAccessor.OnUpdated(partition => m_partition = partition);
		}

		public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
		{
			if (m_partition == null)
			{
				return Task.CompletedTask;
			}

			m_partition.ReportPartitionHealth(new SfHealthInformation(SourceId, "Summary", ConvertStatus(report.Status)));

			foreach(KeyValuePair<string, HealthReportEntry> entryPair in report.Entries)
			{
				HealthReportEntry entry = entryPair.Value;

				m_partition.ReportPartitionHealth(new SfHealthInformation(SourceId, entryPair.Key, ConvertStatus(entry.Status))
				{
					Description = entry.Description
				});
			}

			return Task.CompletedTask;
		}

		private SfHealthState ConvertStatus(HealthStatus state) =>
			state switch
			{
				HealthStatus.Healthy => SfHealthState.Ok,
				HealthStatus.Degraded => SfHealthState.Warning,
				HealthStatus.Unhealthy => SfHealthState.Error,
				_ => SfHealthState.Invalid,
			};
	}
}
