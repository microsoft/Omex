// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.ServiceFabric.Client;
using Microsoft.ServiceFabric.Client.Http;
using ServiceFabricCommon = Microsoft.ServiceFabric.Common;
using ServiceFabricHealth = System.Fabric.Health;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class RestHealthCheckPublisher : HealthCheckPublisher
	{
		private readonly ServiceFabricHttpClient m_client;
		private readonly ILogger<ServiceFabricHealthCheckPublisher> m_logger;
		protected override string HealthReportSourceIdImpl => nameof(RestHealthCheckPublisher);

		// Taken from https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-environment-variables-reference
		internal static string FabricNodeNameEnv = "Fabric_NodeName";

		public RestHealthCheckPublisher(ILogger<ServiceFabricHealthCheckPublisher> logger,
										RestHealthCheckPublisherOptions options) : base()
		{
			m_logger = logger;
			m_client = (ServiceFabricHttpClient)new ServiceFabricClientBuilder()
				.UseEndpoints(new Uri(options.RestHealthPublisherClusterEndpoint))
				.BuildAsync().GetAwaiter().GetResult();
		}

		public override async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
		{
			try
			{
				// We trust the framework to ensure that the report is not null and doesn't contain null entries.
				foreach (KeyValuePair<string, HealthReportEntry> entryPair in report.Entries)
				{
					cancellationToken.ThrowIfCancellationRequested();
					await PublishHealthInfoAsync(BuildSfHealthInformation(entryPair.Key, entryPair.Value), cancellationToken);
				}

				cancellationToken.ThrowIfCancellationRequested();
				await PublishHealthInfoAsync(BuildSfHealthInformation(report), cancellationToken);
			}
			catch (Exception)
			{
				// Ignore, the service instance is closing.
			}
		}

		internal ServiceFabricCommon.HealthInformation FromSfHealthInformation(ServiceFabricHealth.HealthInformation healthInfo)
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

		internal async Task PublishHealthInfoAsync(ServiceFabricHealth.HealthInformation healthInfo, CancellationToken cancellationToken)
		{
			ServiceFabricCommon.HealthInformation sfcHealthInfo = FromSfHealthInformation(healthInfo);
			string? nodeName = Environment.GetEnvironmentVariable(FabricNodeNameEnv);
			if(nodeName == null)
			{
				m_logger.LogError(Tag.Create(), "Can't find node name.");
				return;
			}

			await m_client.Nodes.ReportNodeHealthAsync(
				nodeName: nodeName,
				healthInformation: sfcHealthInfo,
				cancellationToken: cancellationToken);
		}
	}
}
