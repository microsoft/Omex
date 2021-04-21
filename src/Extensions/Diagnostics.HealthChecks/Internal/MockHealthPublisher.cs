// Copyright (C) Microsoft Corporation. All rights reserved.

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

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Mtyrolski
{
	/// <summary>
	/// Just mock
	/// </summary>
	public class MockHealthPublisher : IHealthCheckPublisher
	{
		private readonly ServiceFabricHttpClient m_client;
		private readonly StringBuilder m_descriptionBuilder;
		private readonly string m_serviceId;
		internal const string HealthReportSourceId = nameof(MockHealthPublisher);
		internal const string HealthReportSummaryProperty = "HealthReportSummary";


		/// <summary>
		/// 
		/// </summary>
		public MockHealthPublisher(Uri clusterEndpoints, string serviceId)
		{
			m_serviceId = serviceId;
			m_descriptionBuilder = new();
			m_client = (ServiceFabricHttpClient)new ServiceFabricClientBuilder()
				.UseEndpoints(clusterEndpoints)
				.BuildAsync().GetAwaiter().GetResult();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="healthCheckName"></param>
		/// <param name="reportEntry"></param>
		/// <returns></returns>
		private sfh.HealthInformation BuildSfHealthInformation(string healthCheckName, HealthReportEntry reportEntry)
		{
			m_descriptionBuilder.Clear();
			if (!string.IsNullOrWhiteSpace(reportEntry.Description))
			{
				m_descriptionBuilder.Append("Description: ").Append(reportEntry.Description).AppendLine();
			}
			m_descriptionBuilder.Append("Duration: ").Append(reportEntry.Duration).Append('.').AppendLine();
			if (reportEntry.Exception != null)
			{
				m_descriptionBuilder.Append("Exception: ").Append(reportEntry.Exception).AppendLine();
			}

			string description = m_descriptionBuilder.ToString();

			return new sfh.HealthInformation(HealthReportSourceId, healthCheckName, ToSfHealthState(reportEntry.Status))
			{
				Description = description,
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="report"></param>
		/// <returns></returns>
		private sfh.HealthInformation BuildSfHealthInformation(HealthReport report)
		{
			int entriesCount = report.Entries.Count;
			int healthyEntries = 0;
			int degradedEntries = 0;
			int unhealthyEntries = 0;
			foreach (KeyValuePair<string, HealthReportEntry> entryPair in report.Entries)
			{
				switch (entryPair.Value.Status)
				{
					case HealthStatus.Healthy:
						healthyEntries++;
						break;
					case HealthStatus.Degraded:
						degradedEntries++;
						break;
					case HealthStatus.Unhealthy:
					default:
						unhealthyEntries++;
						break;
				}
			}

			m_descriptionBuilder
				.AppendFormat("Health checks executed: {0}. ", entriesCount)
				.AppendFormat("Healthy: {0}/{1}. ", healthyEntries, entriesCount)
				.AppendFormat("Degraded: {0}/{1}. ", degradedEntries, entriesCount)
				.AppendFormat("Unhealthy: {0}/{1}.", unhealthyEntries, entriesCount)
				.AppendLine()
				.AppendFormat("Total duration: {0}.", report.TotalDuration)
				.AppendLine();

			string description = m_descriptionBuilder.ToString();

			return new sfh.HealthInformation(HealthReportSourceId, HealthReportSummaryProperty, ToSfHealthState(report.Status))
			{
				Description = description,
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="healthStatus"></param>
		/// <returns></returns>
		private sfh.HealthState ToSfHealthState(HealthStatus healthStatus) =>
			healthStatus switch
			{
				HealthStatus.Healthy => sfh.HealthState.Ok,
				HealthStatus.Degraded => sfh.HealthState.Warning,
				HealthStatus.Unhealthy => sfh.HealthState.Error,
				_ => throw new ArgumentException($"'{healthStatus}' is not a valid health status."),
			};

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
		public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
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
