// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions;
using Microsoft.ServiceFabric.Client;
using Microsoft.ServiceFabric.Common;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class RestHealthStatusSender : IHealthStatusSender
	{
		private const string HealthReportSourceId = nameof(RestHealthStatusSender);

		private readonly IServiceFabricClientWrapper m_clientWrapper;

		private readonly string? m_nodeName;

		private IServiceFabricClient? m_client;

		public RestHealthStatusSender(IServiceFabricClientWrapper clientWrapper, IExecutionContext executionContext)
		{
			m_clientWrapper = clientWrapper;
			m_nodeName = executionContext.NodeName;
		}

		public async Task IntializeAsync(CancellationToken token)
		{
			if (m_client == null)
			{
				m_client = await m_clientWrapper.GetAsync();
			}
		}

		public async Task SendStatusAsync(string checkName, HealthStatus status, string description, CancellationToken token)
		{
			_ = m_client ?? throw new InvalidOperationException("ServiceFabricClient not available");

			await m_client.Nodes.ReportNodeHealthAsync(
				nodeName: m_nodeName,
				healthInformation: new HealthInformation(HealthReportSourceId, checkName, ToSfHealthState(status), description: description));
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
