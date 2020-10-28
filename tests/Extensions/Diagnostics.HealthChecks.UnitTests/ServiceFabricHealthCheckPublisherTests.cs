// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SfHealthInformation = System.Fabric.Health.HealthInformation;
using SfHealthState = System.Fabric.Health.HealthState;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class ServiceFabricHealthCheckPublisherTests
	{
		private ILogger<ServiceFabricHealthCheckPublisher> NullLogger { get; } = new NullLogger<ServiceFabricHealthCheckPublisher>();

		[TestMethod]
		public async Task PublishAsync_WhenPartitionIsNotAvailable_ReturnsGracefully()
		{
			// Arrange.
			Accessor<IServicePartition> partitionAccessor = new Accessor<IServicePartition>(null);
			ServiceFabricHealthCheckPublisher publisher = new ServiceFabricHealthCheckPublisher(partitionAccessor, NullLogger);

			// Act.
			await publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, cancellationToken: default);

			// Assert.
			// If we are here, PublishAsync didn't explode.
		}

		[TestMethod]
		public async Task PublishAsync_ForStatefulServicePartition_ReportsReplicaHealth()
		{
			// Arrange.
			Mock<IStatefulServicePartition> mockPartition = new Mock<IStatefulServicePartition>();
			Accessor<IServicePartition> partitionAccessor = new Accessor<IServicePartition>(mockPartition.Object);
			ServiceFabricHealthCheckPublisher publisher = new ServiceFabricHealthCheckPublisher(partitionAccessor, NullLogger);

			// Act.
			await publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, cancellationToken: default);

			// Assert.
			mockPartition.Verify(partition => partition.ReportReplicaHealth(It.IsAny<SfHealthInformation>()));
		}

		[TestMethod]
		public async Task PublishAsync_ForStatelessServicePartition_ReportsInstanceHealth()
		{
			// Arrange.
			Mock<IStatelessServicePartition> mockPartition = new Mock<IStatelessServicePartition>();
			Accessor<IServicePartition> partitionAccessor = new Accessor<IServicePartition>(mockPartition.Object);
			ServiceFabricHealthCheckPublisher publisher = new ServiceFabricHealthCheckPublisher(partitionAccessor, NullLogger);

			// Act.
			await publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, cancellationToken: default);

			// Assert.
			mockPartition.Verify(partition => partition.ReportInstanceHealth(It.IsAny<SfHealthInformation>()));
		}

		[TestMethod]
		public async Task PublishAsync_ForUnknownPartitionType_ThrowsArgumentException()
		{
			// Arrange.
			Mock<IServicePartition> mockPartition = new Mock<IServicePartition>();
			Accessor<IServicePartition> partitionAccessor = new Accessor<IServicePartition>(mockPartition.Object);
			ServiceFabricHealthCheckPublisher publisher = new ServiceFabricHealthCheckPublisher(partitionAccessor, NullLogger);

			// Act.
			async Task act() => await publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, cancellationToken: default);

			// Assert.
			ArgumentException actualException = await Assert.ThrowsExceptionAsync<ArgumentException>(act);
			string partitionTypeName = mockPartition.Object.GetType().ToString();
			StringAssert.Contains(actualException.Message, partitionTypeName);
		}

		[DataTestMethod]
		[DataRow("TestHealthyCheck", HealthStatus.Healthy, SfHealthState.Ok)]
		[DataRow("TestDegradedCheck", HealthStatus.Degraded, SfHealthState.Warning)]
		[DataRow("TestUnhealthyCheck", HealthStatus.Unhealthy, SfHealthState.Error)]
		public async Task PublishAsync_ForHealthCheckWithValidHealthStatus_ReportsCorrectSfHealthState(string healthCheckName, HealthStatus healthCheckStatus, SfHealthState expectedSfHealthState)
		{
			// Arrange.
			SfHealthInformation? reportedSfHealthInformation = null;

			Mock<IStatelessServicePartition> mockPartition = new Mock<IStatelessServicePartition>();
			mockPartition.Setup(partition => partition.ReportInstanceHealth(It.Is<SfHealthInformation>(info => info.Property == healthCheckName)))
				.Callback<SfHealthInformation>(info => reportedSfHealthInformation = info);
			Accessor<IServicePartition> partitionAccessor = new Accessor<IServicePartition>(mockPartition.Object);
			ServiceFabricHealthCheckPublisher publisher = new ServiceFabricHealthCheckPublisher(partitionAccessor, NullLogger);

			HealthReport report = new HealthReportBuilder()
				.AddEntry(healthCheckName, healthCheckStatus)
				.ToHealthReport();

			// Act.
			await publisher.PublishAsync(report, cancellationToken: default);

			// Assert.
			Assert.AreEqual(expectedSfHealthState, reportedSfHealthInformation?.HealthState);
		}

		[DataTestMethod]
		[DataRow("NegativeInvalidStatusCheck", -1)]
		[DataRow("PositiveInvalidStatusCheck", 3)]
		public async Task PublishAsync_ForHealthCheckWithInvalidHealthStatus_ThrowsArgumentException(string healthCheckName, HealthStatus invalidHealthCheckStatus)
		{
			// Arrange.
			Mock<IStatelessServicePartition> mockPartition = new Mock<IStatelessServicePartition>();
			Accessor<IServicePartition> partitionAccessor = new Accessor<IServicePartition>(mockPartition.Object);
			ServiceFabricHealthCheckPublisher publisher = new ServiceFabricHealthCheckPublisher(partitionAccessor, NullLogger);

			HealthReport report = new HealthReportBuilder()
				.AddEntry(healthCheckName, invalidHealthCheckStatus)
				.ToHealthReport();

			// Act.
			async Task act() => await publisher.PublishAsync(report, cancellationToken: default);

			// Assert.
			ArgumentException actualException = await Assert.ThrowsExceptionAsync<ArgumentException>(act);
			StringAssert.Contains(actualException.Message, invalidHealthCheckStatus.ToString());
		}

		[DataTestMethod]
		[DataRow("TestHealthyCheck", HealthStatus.Healthy, SfHealthState.Ok)]
		[DataRow("TestDegradedCheck", HealthStatus.Degraded, SfHealthState.Warning)]
		[DataRow("TestUnhealthyCheck", HealthStatus.Unhealthy, SfHealthState.Error)]
		public async Task PublishAsync_ForReportWithValidHealthStatus_ReportsCorrectSfHealthState(string healthCheckName, HealthStatus healthCheckStatus, SfHealthState expectedSfHealthState)
		{
			// Arrange.
			SfHealthInformation? reportedSfHealthInformation = null;

			Mock<IStatelessServicePartition> mockPartition = new Mock<IStatelessServicePartition>();
			mockPartition.Setup(partition => partition.ReportInstanceHealth(It.Is<SfHealthInformation>(info => info.Property == ServiceFabricHealthCheckPublisher.HealthReportSummaryProperty)))
				.Callback<SfHealthInformation>(info => reportedSfHealthInformation = info);
			Accessor<IServicePartition> partitionAccessor = new Accessor<IServicePartition>(mockPartition.Object);
			ServiceFabricHealthCheckPublisher publisher = new ServiceFabricHealthCheckPublisher(partitionAccessor, NullLogger);

			// Currently report status is calculated as the worst of all healthchecks. In .NET 5 it will be possible to set it independently.
			HealthReport report = new HealthReportBuilder()
				.AddEntry(healthCheckName, healthCheckStatus)
				.ToHealthReport();

			// Act.
			await publisher.PublishAsync(report, cancellationToken: default);

			// Assert.
			Assert.AreEqual(expectedSfHealthState, reportedSfHealthInformation?.HealthState);
		}

		[DataTestMethod]
		[DataRow("NegativeInvalidStatusCheck", -1)]
		[DataRow("PositiveInvalidStatusCheck", 3)]
		public async Task PublishAsync_ForReportWithInvalidHealthStatus_ThrowsArgumentException(string healthCheckName, HealthStatus invalidHealthCheckStatus)
		{
			// Arrange.
			Mock<IStatelessServicePartition> mockPartition = new Mock<IStatelessServicePartition>();
			Accessor<IServicePartition> partitionAccessor = new Accessor<IServicePartition>(mockPartition.Object);
			ServiceFabricHealthCheckPublisher publisher = new ServiceFabricHealthCheckPublisher(partitionAccessor, NullLogger);

			// Currently report status is calculated as the worst of all entries. In .NET 5 it will be possible to set it independently.
			HealthReport report = new HealthReportBuilder()
				.AddEntry(healthCheckName, invalidHealthCheckStatus)
				.ToHealthReport();

			// Act.
			async Task act() => await publisher.PublishAsync(report, cancellationToken: default);

			// Assert.
			ArgumentException actualException = await Assert.ThrowsExceptionAsync<ArgumentException>(act);
			StringAssert.Contains(actualException.Message, invalidHealthCheckStatus.ToString());
		}

		[TestMethod]
		public async Task PublishAsync_WhenCancellationTokenIsCancelled_ThrowsOperationCanceledException()
		{
			// Arrange.
			Mock<IStatefulServicePartition> mockPartition = new Mock<IStatefulServicePartition>();
			Accessor<IServicePartition> partitionAccessor = new Accessor<IServicePartition>(mockPartition.Object);
			ServiceFabricHealthCheckPublisher publisher = new ServiceFabricHealthCheckPublisher(partitionAccessor, NullLogger);

			CancellationTokenSource tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();

			// Act.
			async Task act() => await publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, tokenSource.Token);

			// Assert.
			OperationCanceledException actualException = await Assert.ThrowsExceptionAsync<OperationCanceledException>(act);
		}

		[TestMethod]
		public async Task PublishAsync_WhenStatelessPartitionIsClosed_ReturnsGracefully()
		{
			// Arrange.
			Mock<IStatelessServicePartition> mockPartition = new Mock<IStatelessServicePartition>();
			mockPartition.Setup(partition => partition.ReportInstanceHealth(It.IsAny<SfHealthInformation>()))
				.Throws<FabricObjectClosedException>();
			Accessor<IServicePartition> partitionAccessor = new Accessor<IServicePartition>(mockPartition.Object);
			ServiceFabricHealthCheckPublisher publisher = new ServiceFabricHealthCheckPublisher(partitionAccessor, NullLogger);

			// Act.
			await publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, cancellationToken: default);

			// Assert.
			// If we are here, PublishAsync didn't explode.
		}

		[TestMethod]
		public async Task PublishAsync_WhenStatefulPartitionIsClosed_ReturnsGracefully()
		{
			// Arrange.
			Mock<IStatefulServicePartition> mockPartition = new Mock<IStatefulServicePartition>();
			mockPartition.Setup(partition => partition.ReportReplicaHealth(It.IsAny<SfHealthInformation>()))
				.Throws<FabricObjectClosedException>();
			Accessor<IServicePartition> partitionAccessor = new Accessor<IServicePartition>(mockPartition.Object);
			ServiceFabricHealthCheckPublisher publisher = new ServiceFabricHealthCheckPublisher(partitionAccessor, NullLogger);

			// Act.
			await publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, cancellationToken: default);

			// Assert.
			// If we are here, PublishAsync didn't explode.
		}

		private class HealthReportBuilder
		{
			private Dictionary<string, HealthReportEntry> Entries { get; } = new Dictionary<string, HealthReportEntry>();

			private TimeSpan TotalDuration { get; set; }

			public static HealthReport EmptyHealthReport { get; } = new HealthReport(new Dictionary<string, HealthReportEntry>(), totalDuration: default);

			public HealthReportBuilder AddEntry(string healthCheckName, HealthStatus status)
			{
				HealthReportEntry entry = new HealthReportEntry(status, description: null, duration: default, exception: null, data: null, tags: null);
				Entries.Add(healthCheckName, entry); // Health report shouldn't contain duplicate entries.
				return this;
			}

			public HealthReportBuilder SetTotalDuration(TimeSpan totalDuration)
			{
				TotalDuration = totalDuration;
				return this;
			}

			public HealthReport ToHealthReport()
			{
				return new HealthReport(Entries, TotalDuration);
			}
		}
	}
}
