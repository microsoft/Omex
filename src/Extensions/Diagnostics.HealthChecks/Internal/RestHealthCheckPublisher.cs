// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.ServiceFabric.Client;
using Microsoft.ServiceFabric.Client.Http;
using sfc = Microsoft.ServiceFabric.Common;
using sfh = System.Fabric.Health;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	/// <summary>
	/// Just mock
	/// </summary>
	public class RestHealthCheckPublisher : HealthCheckPublisher
	{
		private readonly ServiceFabricHttpClient m_client;
		private readonly string m_serviceId;
		private StringBuilder m_descriptionBuilder;
		/// <summary>
		/// 
		/// </summary>
		protected override string HealthReportSourceIdImpl => nameof(RestHealthCheckPublisher);

		/// <summary>
		/// 
		/// </summary>
		public RestHealthCheckPublisher(Uri clusterEndpoints, string serviceId)
		{
			m_descriptionBuilder = new();
			m_serviceId = serviceId;
			m_client = (ServiceFabricHttpClient)new ServiceFabricClientBuilder()
				.UseEndpoints(clusterEndpoints)
				.BuildAsync().GetAwaiter().GetResult();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="healthInfo"></param>
		/// <returns></returns>
		private sfc.HealthInformation fromSfHealthInformation(sfh.HealthInformation healthInfo)
		{
			sfc.HealthState healthState = healthInfo.HealthState switch
			{
				sfh.HealthState.Invalid => sfc.HealthState.Invalid,
				sfh.HealthState.Ok => sfc.HealthState.Ok,
				sfh.HealthState.Warning => sfc.HealthState.Warning,
				sfh.HealthState.Error => sfc.HealthState.Error,
				sfh.HealthState.Unknown => sfc.HealthState.Unknown,
				_ => throw new ArgumentException($"'{healthInfo.HealthState}' is not a valid health status."),
			};

			sfc.HealthInformation sfcHealthInfo = new(
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

		private async Task PublishHealthInfo(sfh.HealthInformation healthInfo, CancellationToken cancellationToken)
		{
			IExecutionContext ctx = new BaseExecutionContext();
			IPAddress addr = ctx.ClusterIpAddress;

			sfc.HealthInformation sfcHealthInfo = fromSfHealthInformation(healthInfo);
			await m_client.Services.ReportServiceHealthAsync(
					serviceId: m_serviceId,
					healthInformation: sfcHealthInfo
				).ConfigureAwait(false);
		}

		/// <summary>
		/// Just mock
		/// </summary>
		/// <param name="report"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public override async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
		{
			try
			{
				// We trust the framework to ensure that the report is not null and doesn't contain null entries.
				foreach (KeyValuePair<string, HealthReportEntry> entryPair in report.Entries)
				{
					cancellationToken.ThrowIfCancellationRequested();
					await PublishHealthInfo(BuildSfHealthInformation(entryPair.Key, entryPair.Value), cancellationToken);
				}

				cancellationToken.ThrowIfCancellationRequested();
				await PublishHealthInfo(BuildSfHealthInformation(report), cancellationToken);
			}
			catch (Exception)
			{
				// Ignore, the service instance is closing.
			}
		}
	}
}
