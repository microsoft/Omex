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
using Microsoft.Extensions.ObjectPool;
using Microsoft.Omex.Extensions.Abstractions;
using SfHealthInformation = System.Fabric.Health.HealthInformation;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class ServiceFabricHealthCheckPublisher : HealthCheckPublisher
	{
		private readonly IAccessor<IServicePartition> m_partitionAccessor;

		private readonly ILogger<ServiceFabricHealthCheckPublisher> m_logger;

		public ServiceFabricHealthCheckPublisher(
			IAccessor<IServicePartition> partitionAccessor,
			ILogger<ServiceFabricHealthCheckPublisher> logger,
			ObjectPoolProvider objectPoolProvider) : base(objectPoolProvider)
		{
			m_partitionAccessor = partitionAccessor;
			m_logger = logger;
		}

		internal static new string? HealthReportSourceId => nameof(ServiceFabricHealthCheckPublisher);

		public override Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
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
				_ => throw new ArgumentException($"Service partition type '{partition.GetType()}' is not supported."),
			};

			PublishAllEntries(report, reportHealth, cancellationToken);

			return Task.CompletedTask;
		}
	}
}
