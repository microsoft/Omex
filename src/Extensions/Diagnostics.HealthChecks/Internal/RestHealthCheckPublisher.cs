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

			Action<HealthStatus, string> reportHealthWithConvert = new(async (status, description) =>
			{
				await PublishHealthInfoAsync(new ServiceFabricCommon.HealthInformation(HealthReportSourceId, description, ToSfcHealthState(status)));
			});

			PublishAllEntries(report, reportHealthWithConvert, cancellationToken);
		}

		private ServiceFabricCommon.HealthState ToSfcHealthState(HealthStatus healthStatus) =>
			healthStatus switch
			{
				HealthStatus.Healthy => ServiceFabricCommon.HealthState.Ok,
				HealthStatus.Degraded => ServiceFabricCommon.HealthState.Warning,
				HealthStatus.Unhealthy => ServiceFabricCommon.HealthState.Error,
				_ => throw new ArgumentException($"'{healthStatus}' is not a valid health status."),
			};


		internal async Task PublishHealthInfoAsync(ServiceFabricCommon.HealthInformation sfcHealthInfo)
		{
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
