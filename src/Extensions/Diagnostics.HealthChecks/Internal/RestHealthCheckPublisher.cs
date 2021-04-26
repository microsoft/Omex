// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.ServiceFabric.Client.Http;
using ServiceFabricCommon = Microsoft.ServiceFabric.Common;
using ServiceFabricHealth = System.Fabric.Health;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class RestHealthCheckPublisher : HealthCheckPublisher
	{
		private readonly RestClientWrapper m_clientWrapper;

		private readonly ILogger<ServiceFabricHealthCheckPublisher> m_logger;

		private string? m_nodeName;

		protected override string HealthReportSourceIdImpl => nameof(RestHealthCheckPublisher);

		internal const string NodeNameVariableName = "Fabric_NodeName";

		public RestHealthCheckPublisher(ILogger<ServiceFabricHealthCheckPublisher> logger,
										IOptions<RestHealthCheckPublisherOptions> options,
										ObjectPoolProvider objectPoolProvider) : base(objectPoolProvider)
		{
			m_nodeName = FindNodeName();
			m_clientWrapper = new(new Uri(options.Value.RestHealthPublisherClusterEndpoint));
			m_logger = logger;
		}

		public override async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
		{
			if(m_nodeName == null)
			{
				m_logger.LogError(Tag.Create(), "Can't find node name.");
				return;
			}

			ServiceFabricHttpClient httpClient = await m_clientWrapper.GetAsync();
			try
			{
				// We trust the framework to ensure that the report is not null and doesn't contain null entries.
				foreach (KeyValuePair<string, HealthReportEntry> entryPair in report.Entries)
				{
					cancellationToken.ThrowIfCancellationRequested();
					await PublishHealthInfoAsync(BuildSfHealthInformation(entryPair.Key, entryPair.Value), cancellationToken, httpClient);
				}

				cancellationToken.ThrowIfCancellationRequested();
				await PublishHealthInfoAsync(BuildSfHealthInformation(report), cancellationToken, httpClient);
			}
			catch (Exception)
			{
				// Ignore, the service instance is closing.
			}
		}

		internal static ServiceFabricCommon.HealthInformation FromSfHealthInformation(ServiceFabricHealth.HealthInformation healthInfo)
		{
			ServiceFabricCommon.HealthState healthState = healthInfo.HealthState switch
			{
				ServiceFabricHealth.HealthState.Invalid => ServiceFabricCommon.HealthState.Invalid,
				ServiceFabricHealth.HealthState.Ok => ServiceFabricCommon.HealthState.Ok,
				ServiceFabricHealth.HealthState.Warning => ServiceFabricCommon.HealthState.Warning,
				ServiceFabricHealth.HealthState.Error => ServiceFabricCommon.HealthState.Error,
				ServiceFabricHealth.HealthState.Unknown => ServiceFabricCommon.HealthState.Unknown,
				_ => throw new ArgumentException($"'{healthInfo.HealthState}' is not a valid health status."),
			};

			ServiceFabricCommon.HealthInformation sfcHealthInfo = new(
				   sourceId: healthInfo.SourceId,
				   property: healthInfo.Property,
				   healthState: healthState,
				   timeToLiveInMilliSeconds: healthInfo.TimeToLive,
				   description: healthInfo.Description,
				   sequenceNumber: healthInfo.SequenceNumber.ToString(),
				   removeWhenExpired: healthInfo.RemoveWhenExpired,
				   healthReportId: healthInfo.HealthReportId
		   );

			return sfcHealthInfo;
		}

		internal async Task PublishHealthInfoAsync(ServiceFabricHealth.HealthInformation healthInfo, CancellationToken cancellationToken,
			ServiceFabricHttpClient httpClient)
		{
			ServiceFabricCommon.HealthInformation sfcHealthInfo = FromSfHealthInformation(healthInfo);

			await httpClient.Nodes.ReportNodeHealthAsync(
				nodeName: m_nodeName,
				healthInformation: sfcHealthInfo,
				cancellationToken: cancellationToken);
		}

		internal string? FindNodeName() => Environment.GetEnvironmentVariable(NodeNameVariableName);
	}
}
