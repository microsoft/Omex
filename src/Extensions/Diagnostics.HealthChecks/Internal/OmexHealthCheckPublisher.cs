// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class OmexHealthCheckPublisher : IHealthCheckPublisher
	{
		protected readonly ObjectPool<StringBuilder> StringBuilderPool;

		private IHealthStatusSender StatusSender { get; }

		internal static readonly string HealthReportSummaryProperty = "HealthReportSummary";

		public OmexHealthCheckPublisher(IHealthStatusSender statusSender, ObjectPoolProvider objectPoolProvider)
		{
			StringBuilderPool = objectPoolProvider.CreateStringBuilderPool();
			StatusSender = statusSender;
		}

		public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
		{
			bool isInitialized = await StatusSender.IntializeAsync(cancellationToken).ConfigureAwait(false);
			if (!isInitialized)
			{
				return;
			}

			// We trust the framework to ensure that the report is not null and doesn't contain null entries.
			foreach (KeyValuePair<string, HealthReportEntry> entryPair in report.Entries)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await StatusSender.SendStatusAsync(entryPair.Key, entryPair.Value.Status, BuildSfHealthInformationDescription(entryPair.Value), cancellationToken)
					.ConfigureAwait(false);
			}

			cancellationToken.ThrowIfCancellationRequested();
			await StatusSender.SendStatusAsync(HealthReportSummaryProperty, report.Status, BuildHealthSummaryDescription(report), cancellationToken)
				.ConfigureAwait(false);
		}

		protected string BuildHealthSummaryDescription(HealthReport report)
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

			StringBuilder descriptionBuilder = StringBuilderPool.Get();
			string description;

			try
			{
				descriptionBuilder
					.AppendFormat("Health checks executed: {0}. ", entriesCount)
					.AppendFormat("Healthy: {0}/{1}. ", healthyEntries, entriesCount)
					.AppendFormat("Degraded: {0}/{1}. ", degradedEntries, entriesCount)
					.AppendFormat("Unhealthy: {0}/{1}.", unhealthyEntries, entriesCount)
					.AppendLine()
					.AppendFormat("Total duration: {0}.", report.TotalDuration)
					.AppendLine();

				description = descriptionBuilder.ToString();
			}
			finally
			{
				StringBuilderPool.Return(descriptionBuilder);
			}

			return description;
		}

		protected string BuildSfHealthInformationDescription(HealthReportEntry reportEntry)
		{
			StringBuilder descriptionBuilder = StringBuilderPool.Get();
			string description;
			try
			{
				if (!string.IsNullOrWhiteSpace(reportEntry.Description))
				{
					descriptionBuilder.Append("Description: ").Append(reportEntry.Description).AppendLine();
				}
				descriptionBuilder.Append("Duration: ").Append(reportEntry.Duration).Append('.').AppendLine();
				if (reportEntry.Exception != null)
				{
					descriptionBuilder.Append("Exception: ").Append(reportEntry.Exception).AppendLine();
				}

				description = descriptionBuilder.ToString();
			}
			finally
			{
				StringBuilderPool.Return(descriptionBuilder);
			}

			return description;
		}
	}
}
