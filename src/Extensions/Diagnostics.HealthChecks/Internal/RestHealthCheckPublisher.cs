// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions;
using Microsoft.ServiceFabric.Client;
using ServiceFabricCommon = Microsoft.ServiceFabric.Common;
using ServiceFabricHealth = System.Fabric.Health;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class RestHealthCheckPublisher : HealthCheckPublisher
	{
		private readonly IServiceFabricClientWrapper m_clientWrapper;

		private IServiceFabricClient? m_client;

		private readonly ILogger<RestHealthCheckPublisher> m_logger;

		private string? m_nodeName;

		internal override string HealthReportSourceId => nameof(RestHealthCheckPublisher);

		internal const string NodeNameVariableName = "Fabric_NodeName";

		public RestHealthCheckPublisher(ILogger<RestHealthCheckPublisher> logger,
			IServiceFabricClientWrapper clientWrapper,
			ObjectPoolProvider objectPoolProvider) : base(objectPoolProvider)
		{
			m_nodeName = FindNodeName();
			m_clientWrapper = clientWrapper;
			m_logger = logger;
		}


		internal RestHealthCheckPublisher(ILogger<RestHealthCheckPublisher> logger,
			IServiceFabricClientWrapper clientWrapper,
			ObjectPoolProvider objectPoolProvider,
			string nodeName) : base(objectPoolProvider)
		{
			m_nodeName = nodeName;
			m_clientWrapper = clientWrapper;
			m_logger = logger;
		}

		public override async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
		{
			if (m_nodeName == null)
			{
				m_logger.LogError(Tag.Create(), "Can't find node name.");
				return;
			}

			m_client = await m_clientWrapper.GetAsync();

			Action<ServiceFabricHealth.HealthInformation> reportHealth =
				async (sfHealthInfo) => await PublishHealthInfoAsync(sfHealthInfo);

			PublishAllEntries(report, reportHealth, cancellationToken);
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

		internal async Task PublishHealthInfoAsync(ServiceFabricHealth.HealthInformation healthInfo)
		{
			ServiceFabricCommon.HealthInformation sfcHealthInfo = FromSfHealthInformation(healthInfo);

			if (m_client == null)
			{
				m_logger.LogInformation(Tag.Create(), "HTTP Client isn't ready yet.");
				return;
			}

			await m_client.Nodes.ReportNodeHealthAsync(
				nodeName: m_nodeName,
				healthInformation: sfcHealthInfo);
		}

		internal string? FindNodeName() => Environment.GetEnvironmentVariable(NodeNameVariableName);
	}
}
