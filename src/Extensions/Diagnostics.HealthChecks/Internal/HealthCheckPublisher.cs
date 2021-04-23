// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using sfh = System.Fabric.Health;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal abstract class HealthCheckPublisher : IHealthCheckPublisher
	{
		protected StringBuilder m_descriptionBuilder;

		protected string HealthReportSourceId { get { return HealthReportSourceIdImpl; } }

		protected abstract string HealthReportSourceIdImpl { get; }

		protected const string HealthReportSummaryProperty = "HealthReportSummary";

		public HealthCheckPublisher()
		{
			m_descriptionBuilder = new();
		}

		public abstract Task PublishAsync(HealthReport report, CancellationToken cancellationToken);

		protected sfh.HealthInformation BuildSfHealthInformation(HealthReport report)
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

		protected sfh.HealthState ToSfHealthState(HealthStatus healthStatus) =>
			healthStatus switch
			{
				HealthStatus.Healthy => sfh.HealthState.Ok,
				HealthStatus.Degraded => sfh.HealthState.Warning,
				HealthStatus.Unhealthy => sfh.HealthState.Error,
				_ => throw new ArgumentException($"'{healthStatus}' is not a valid health status."),
			};

		protected sfh.HealthInformation BuildSfHealthInformation(string healthCheckName, HealthReportEntry reportEntry)
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
	}
}
