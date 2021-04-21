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
	/// <summary>
	/// 
	/// </summary>
	public class ServiceFabricHealthCheckPublisher : HealthCheckPublisher
	{

		private readonly IAccessor<IServicePartition> m_partitionAccessor;

		private readonly ILogger<ServiceFabricHealthCheckPublisher> m_logger;

		private readonly ObjectPool<StringBuilder> m_stringBuilderPool;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="partitionAccessor"></param>
		/// <param name="logger"></param>
		/// <param name="objectPoolProvider"></param>
		public ServiceFabricHealthCheckPublisher(
			IAccessor<IServicePartition> partitionAccessor,
			ILogger<ServiceFabricHealthCheckPublisher> logger,
			ObjectPoolProvider objectPoolProvider) : base()
		{
			m_partitionAccessor = partitionAccessor;
			m_logger = logger;
			m_stringBuilderPool = objectPoolProvider.CreateStringBuilderPool();
		}

		/// <summary>
		/// 
		/// </summary>
		protected override string HealthReportSourceIdImpl => nameof(ServiceFabricHealthCheckPublisher);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="report"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
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

			try
			{
				// We trust the framework to ensure that the report is not null and doesn't contain null entries.
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
	}
}
