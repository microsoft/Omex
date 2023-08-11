// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using System.Fabric.Health;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class ServiceContextHealthStatusSender : IHealthStatusSender
	{
		private const string HealthReportSourceId = nameof(ServiceContextHealthStatusSender);

		private readonly IAccessor<IServicePartition> m_partitionAccessor;

		private readonly ILogger<ServiceContextHealthStatusSender> m_logger;

		private Action<HealthInformation>? m_reportHealth;

		public ServiceContextHealthStatusSender(IAccessor<IServicePartition> partitionAccessor, ILogger<ServiceContextHealthStatusSender> logger)
		{
			m_partitionAccessor = partitionAccessor;
			m_logger = logger;
		}

		public async Task<bool> IntializeAsync(CancellationToken token)
		{
			IServicePartition? partition = m_partitionAccessor.Value;
			if (partition == null)
			{
				m_logger.LogWarning(Tag.Create(), "Status sender run before partition available.");
			}
			else
			{
				// Avoiding repeated pattern matching for each report entry.
				m_reportHealth = partition switch
				{
					IStatefulServicePartition statefulPartition => statefulPartition.ReportReplicaHealth,
					IStatelessServicePartition statelessPartition => statelessPartition.ReportInstanceHealth,
					_ => throw new ArgumentException($"Service partition type '{partition.GetType()}' is not supported."),
				};
			}

			return await Task.FromResult(m_reportHealth != null);
		}

		public Task SendStatusAsync(string checkName, HealthStatus status, string description, CancellationToken token)
		{
			_ = m_reportHealth ?? throw new InvalidOperationException("Status sender was executed before run before it can report result.");

			try
			{
				m_reportHealth(new HealthInformation(HealthReportSourceId, checkName, ToSfHealthState(status))
				{
					Description = description
				});
			}
			catch (FabricObjectClosedException)
			{
				// Ignore, the service instance is closing.
			}

			return Task.CompletedTask;
		}

		private HealthState ToSfHealthState(HealthStatus healthStatus) =>
			healthStatus switch
			{
				HealthStatus.Healthy => HealthState.Ok,
				HealthStatus.Degraded => HealthState.Warning,
				HealthStatus.Unhealthy => HealthState.Error,
				_ => throw new ArgumentException($"'{healthStatus}' is not a valid health status."),
			};
	}
}
