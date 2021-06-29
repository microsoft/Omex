// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.ObjectPool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class OmexHealthCheckPublisherTests
	{
		[TestMethod]
		public async Task PublishAsync_WhenCancellationIsRequested_ThrowsOperationCanceledException()
		{
			// Arrange
			PublisherContext publisherContext = CreatePublisher();

			// Act.
			async Task act() => await publisherContext.Publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, new CancellationToken(true));

			// Assert.
			await Assert.ThrowsExceptionAsync<OperationCanceledException>(act);
		}

		[DataTestMethod]
		[DataRow(HealthStatus.Degraded)]
		[DataRow(HealthStatus.Unhealthy)]
		[DataRow(HealthStatus.Healthy)]
		public async Task PublishAsync_ForReportWithValidHealthStatus_ReportsCorrectHealthState(HealthStatus healthCheckStatus)
		{
			// Arrange
			PublisherContext publisherContext = CreatePublisher();

			string healthCheckName = "Check_" + healthCheckStatus;
			HealthReport report = new HealthReportBuilder()
				.AddEntry(healthCheckName, healthCheckStatus)
				.ToHealthReport();

			// Act.
			await publisherContext.Publisher.PublishAsync(report, cancellationToken: default);

			// Assert.
			Assert.AreEqual(healthCheckStatus, publisherContext.ReportedState[healthCheckName].Status);
			Assert.AreEqual(healthCheckStatus, publisherContext.ReportedState[OmexHealthCheckPublisher.HealthReportSummaryProperty].Status);
		}

		[TestMethod]
		public async Task PublishAsync_ForValidReportWithSeveralEntries_ReportsCorrectSfHealthInformation()
		{
			// Arrange.
			PublisherContext publisherContext = CreatePublisher();

			HealthReport report = new HealthReportBuilder()
				.AddEntry(
					"HealthyCheck",
					new HealthReportEntry(
						status: HealthStatus.Healthy,
						description: "Healthy entry description",
						duration: TimeSpan.FromMilliseconds(12639),
						exception: null,
						data: null))
				.AddEntry("DegradedCheck",
					new HealthReportEntry(
						status: HealthStatus.Degraded,
						description: "Degraded entry description",
						duration: TimeSpan.FromMilliseconds(111),
						exception: null,
						data: null))
				.AddEntry(
					"UnhealthyCheck",
					new HealthReportEntry(
						status: HealthStatus.Unhealthy,
						description: "Unhealthy entry description",
						duration: TimeSpan.FromMilliseconds(73),
						exception: new InvalidOperationException("Unhealthy check performed invalid operation."),
						data: null))
				.SetTotalDuration(TimeSpan.FromSeconds(72))
				.ToHealthReport();

			// Act.
			await publisherContext.Publisher.PublishAsync(report, cancellationToken: default);

			// Assert.
			foreach (KeyValuePair<string, HealthReportEntry> pair in report.Entries)
			{
				string name = pair.Key;
				HealthReportEntry entry = pair.Value;

				HealthStateInfo info = publisherContext.ReportedState[name];
				Assert.AreEqual(entry.Status, info.Status, $"HealthState from {name} check is incorrect.");
				StringAssert.Contains(info.Description, entry.Description, $"Description from {name} check is not included.");
				StringAssert.Contains(info.Description, entry.Duration.ToString(), $"Duration from {name} check is not included.");
			}

			HealthStateInfo summaryInfo = publisherContext.ReportedState[OmexHealthCheckPublisher.HealthReportSummaryProperty];
			Assert.AreEqual(HealthStatus.Unhealthy, summaryInfo.Status, "HealthState from report is incorrect.");
			StringAssert.Contains(summaryInfo.Description, report.TotalDuration.ToString(), "TotalDuration from report is not included.");
		}

		private PublisherContext CreatePublisher(bool canInitizlize = true)
		{
			Dictionary<string, HealthStateInfo> reportedState = new ();
			Mock<IHealthStatusSender> mockSender = new ();
			mockSender.Setup(s => s.IntializeAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(canInitizlize));
			mockSender.Setup(s => s.SendStatusAsync(It.IsAny<string>(), It.IsAny<HealthStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Callback((string name, HealthStatus status, string description, CancellationToken token) =>
					reportedState[name] = new HealthStateInfo(status, description))
				.Returns(Task.CompletedTask);

			OmexHealthCheckPublisher publisher = new (
				mockSender.Object,
				new DefaultObjectPoolProvider());

			return new (publisher, reportedState);
		}

		private record PublisherContext(
			OmexHealthCheckPublisher Publisher,
			Dictionary<string, HealthStateInfo> ReportedState);

		private record HealthStateInfo(HealthStatus Status, string Description);

		private class HealthReportBuilder
		{
			private Dictionary<string, HealthReportEntry> Entries { get; } = new Dictionary<string, HealthReportEntry>();

			private TimeSpan TotalDuration { get; set; }

			public static HealthReport EmptyHealthReport { get; } = new HealthReport(new Dictionary<string, HealthReportEntry>(), totalDuration: default);

			public HealthReportBuilder AddEntry(string healthCheckName, HealthStatus status)
			{
				HealthReportEntry entry = new(status, description: null, duration: default, exception: null, data: null);
				return AddEntry(healthCheckName, entry);
			}

			public HealthReportBuilder AddEntry(string healthCheckName, HealthReportEntry entry)
			{
				Entries.Add(healthCheckName, entry); // Health report shouldn't contain duplicate health check names.
				return this;
			}

			public HealthReportBuilder SetTotalDuration(TimeSpan totalDuration)
			{
				TotalDuration = totalDuration;
				return this;
			}

			public HealthReport ToHealthReport() => new(Entries, TotalDuration);
		}
	}
}
