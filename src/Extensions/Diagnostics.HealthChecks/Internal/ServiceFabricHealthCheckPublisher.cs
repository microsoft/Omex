// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Omex.Extensions.Abstractions;
using ServiceFabricHealth = System.Fabric.Health;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class ServiceFabricHealthCheckPublisher : HealthCheckPublisher
	{
		private readonly IAccessor<IServicePartition> m_partitionAccessor;

		private readonly ILogger<ServiceFabricHealthCheckPublisher> m_logger;

		internal override string HealthReportSourceId => nameof(ServiceFabricHealthCheckPublisher);

		public ServiceFabricHealthCheckPublisher(
			IAccessor<IServicePartition> partitionAccessor,
			ILogger<ServiceFabricHealthCheckPublisher> logger,
			ObjectPoolProvider objectPoolProvider) : base(objectPoolProvider)
		{
			m_partitionAccessor = partitionAccessor;
			m_logger = logger;
		}

		public override Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
		{
			IServicePartition? partition = m_partitionAccessor.Value;
			if (partition == null)
			{
				m_logger.LogWarning(Tag.Create(), "Publisher run before partition provided.");

				return Task.CompletedTask;
			}

			// Avoiding repeated pattern matching for each report entry.
			Action<ServiceFabricHealth.HealthInformation> reportHealth = partition switch
			{
				IStatefulServicePartition statefulPartition => statefulPartition.ReportReplicaHealth,
				IStatelessServicePartition statelessPartition => statelessPartition.ReportInstanceHealth,
				_ => throw new ArgumentException($"Service partition type '{partition.GetType()}' is not supported."),
			};

			Func<HealthStatus, string, Task> reportHealthWithConvert = new((status, description) =>
			{
				ServiceFabricHealth.HealthInformation healthEntry = new(HealthReportSourceId, HealthReportSummaryProperty, ToSfHealthState(status));
				healthEntry.Description = description;
				reportHealth(healthEntry);
				return Task.CompletedTask;
			});

			try
			{
				PublishAllEntries(report, reportHealthWithConvert, cancellationToken);
			}
			catch (FabricObjectClosedException)
			{
				// Ignore, the service instance is closing.
			}

			return Task.CompletedTask;
		}

		private ServiceFabricHealth.HealthState ToSfHealthState(HealthStatus healthStatus) =>
			healthStatus switch
			{
				HealthStatus.Healthy => ServiceFabricHealth.HealthState.Ok,
				HealthStatus.Degraded => ServiceFabricHealth.HealthState.Warning,
				HealthStatus.Unhealthy => ServiceFabricHealth.HealthState.Error,
				_ => throw new ArgumentException($"'{healthStatus}' is not a valid health status."),
			};
	}
}
