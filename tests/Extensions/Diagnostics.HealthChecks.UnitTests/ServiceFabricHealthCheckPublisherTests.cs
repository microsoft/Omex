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
using Microsoft.Extensions.ObjectPool;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.ServiceFabric.Client;
using Microsoft.ServiceFabric.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SfHealthInformation = System.Fabric.Health.HealthInformation;
using SfcHealthInformation = Microsoft.ServiceFabric.Common.HealthInformation;
using SfHealthState = System.Fabric.Health.HealthState;
using sfcHealthState = Microsoft.ServiceFabric.Common.HealthState;
using Microsoft.Extensions.Options;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class ServiceFabricHealthCheckPublisherTests
	{
		/*[TestMethod]
		public async Task PublishAsync_WhenPartitionIsNotAvailable_ReturnsGracefully()
		{
			// Arrange.
			PublisherTestContext testContext = new PublisherTestContext();

			// Act.
			await testContext.Publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, cancellationToken: default);

			// Assert.
			// If we are here, PublishAsync didn't explode.
		}

		[TestMethod]
		public async Task PublishAsync_WhenStatefulPartitionIsClosed_ReturnsGracefully()
		{
			// Arrange.
			PublisherTestContext testContext = new PublisherTestContext();
			testContext.ConfigureStatefulServicePartition(closed: true);

			// Act.
			await testContext.Publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, cancellationToken: default);

			// Assert.
			// If we are here, PublishAsync didn't explode.
		}

		[TestMethod]
		public async Task PublishAsync_WhenStatelessPartitionIsClosed_ReturnsGracefully()
		{
			// Arrange.
			PublisherTestContext testContext = new PublisherTestContext();
			testContext.ConfigureStatelessServicePartition(closed: true);

			// Act.
			await testContext.Publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, cancellationToken: default);

			// Assert.
			// If we are here, PublishAsync didn't explode.
		}

		[TestMethod]
		public async Task PublishAsync_ForStatefulServicePartition_ReportsReplicaHealth()
		{
			// Arrange.
			PublisherTestContext testContext = new PublisherTestContext();
			testContext.ConfigureStatefulServicePartition();

			// Act.
			await testContext.Publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, cancellationToken: default);

			// Assert.
			testContext.MockStatefulServicePartition!.Verify(partition => partition.ReportReplicaHealth(It.IsAny<SfHealthInformation>()));
		}

		[TestMethod]
		public async Task PublishAsync_ForStatelessServicePartition_ReportsInstanceHealth()
		{
			// Arrange.
			PublisherTestContext testContext = new PublisherTestContext();
			testContext.ConfigureStatelessServicePartition();

			// Act.
			await testContext.Publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, cancellationToken: default);

			// Assert.
			testContext.MockStatelessServicePartition!.Verify(partition => partition.ReportInstanceHealth(It.IsAny<SfHealthInformation>()));
		}

		[TestMethod]
		public async Task PublishAsync_ForUnknownPartitionType_ThrowsArgumentException()
		{
			// Arrange.
			PublisherTestContext testContext = new PublisherTestContext();
			testContext.ConfigureServicePartition();

			// Act.
			async Task act() => await testContext.Publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, cancellationToken: default);

			// Assert.
			ArgumentException actualException = await Assert.ThrowsExceptionAsync<ArgumentException>(act);
			string partitionTypeName = testContext.MockServicePartition!.Object.GetType().ToString();
			StringAssert.Contains(actualException.Message, partitionTypeName);
		}

		[TestMethod]
		public async Task PublishAsync_WhenCancellationIsRequested_ThrowsOperationCanceledException()
		{
			// Arrange.
			PublisherTestContext testContext = new PublisherTestContext();
			testContext.ConfigureStatefulServicePartition();

			CancellationTokenSource tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();

			// Act.
			async Task act() => await testContext.Publisher.PublishAsync(HealthReportBuilder.EmptyHealthReport, tokenSource.Token);

			// Assert.
			await Assert.ThrowsExceptionAsync<OperationCanceledException>(act);
		}

		[DataTestMethod]
		[DataRow("HealthyCheck", HealthStatus.Healthy, SfHealthState.Ok)]
		[DataRow("DegradedCheck", HealthStatus.Degraded, SfHealthState.Warning)]
		[DataRow("UnhealthyCheck", HealthStatus.Unhealthy, SfHealthState.Error)]
		public async Task PublishAsync_ForHealthCheckWithValidHealthStatus_ReportsCorrectSfHealthState(string healthCheckName, HealthStatus healthCheckStatus, SfHealthState expectedSfHealthState)
		{
			// Arrange.
			PublisherTestContext testContext = new PublisherTestContext();
			testContext.ConfigureStatelessServicePartition();

			HealthReport report = new HealthReportBuilder()
				.AddEntry(healthCheckName, healthCheckStatus)
				.ToHealthReport();

			// Act.
			await testContext.Publisher.PublishAsync(report, cancellationToken: default);

			// Assert.
			Assert.AreEqual(expectedSfHealthState, testContext.ReportedSfHealthInformation(healthCheckName)?.HealthState);
		}

		[DataTestMethod]
		[DataRow("NegativeInvalidStatusCheck", -1)]
		[DataRow("PositiveInvalidStatusCheck", 3)]
		public async Task PublishAsync_ForHealthCheckWithInvalidHealthStatus_ThrowsArgumentException(string healthCheckName, HealthStatus invalidHealthCheckStatus)
		{
			// Arrange.
			PublisherTestContext testContext = new PublisherTestContext();
			testContext.ConfigureStatefulServicePartition();

			HealthReport report = new HealthReportBuilder()
				.AddEntry(healthCheckName, invalidHealthCheckStatus)
				.ToHealthReport();

			// Act.
			async Task act() => await testContext.Publisher.PublishAsync(report, cancellationToken: default);

			// Assert.
			ArgumentException actualException = await Assert.ThrowsExceptionAsync<ArgumentException>(act);
			StringAssert.Contains(actualException.Message, invalidHealthCheckStatus.ToString());
		}

		[DataTestMethod]
		[DataRow("HealthyCheck", HealthStatus.Healthy, SfHealthState.Ok)]
		[DataRow("DegradedCheck", HealthStatus.Degraded, SfHealthState.Warning)]
		[DataRow("UnhealthyCheck", HealthStatus.Unhealthy, SfHealthState.Error)]
		public async Task PublishAsync_ForReportWithValidHealthStatus_ReportsCorrectSfHealthState(string healthCheckName, HealthStatus healthCheckStatus, SfHealthState expectedSfHealthState)
		{
			foreach (PublisherTestContext publisherContext in PublishTestManager.TestCases())
			{
				// Arrange.
				publisherContext.Configure();

				// Currently report status is calculated as the worst of all healthchecks. In .NET 5 it will be possible to set it independently.
				HealthReport report = new HealthReportBuilder()
					.AddEntry(healthCheckName, healthCheckStatus)
					.ToHealthReport();

				// Act.
				await publisherContext.Publisher.PublishAsync(report, cancellationToken: default);

				// Assert.
				Assert.AreEqual(expectedSfHealthState, publisherContext.ReportedSfHealthInformation(ServiceFabricHealthCheckPublisher.HealthReportSummaryProperty)?.HealthState);
			}
		}*/

		[DataTestMethod]
		[DataRow("NegativeInvalidStatusCheck", -1)]
		[DataRow("PositiveInvalidStatusCheck", 3)]
		public async Task PublishAsync_ForReportWithInvalidHealthStatus_ThrowsArgumentException(string healthCheckName, HealthStatus invalidHealthCheckStatus)
		{
			foreach (PublisherTestContext publisherContext in PublishTestManager.TestCases())
			{
				// Arrange.
				publisherContext.Configure();

				// Currently report status is calculated as the worst of all entries. In .NET 5 it will be possible to set it independently.
				HealthReport report = new HealthReportBuilder()
					.AddEntry(healthCheckName, invalidHealthCheckStatus)
					.ToHealthReport();
				NullableAssert.IsNotNull(publisherContext.Publisher);

				// Act.
				async Task act() => await publisherContext.Publisher.PublishAsync(report, cancellationToken: default);

				// Assert.
				ArgumentException actualException = await Assert.ThrowsExceptionAsync<ArgumentException>(act);
				StringAssert.Contains(actualException.Message, invalidHealthCheckStatus.ToString());
			}
		}

		[TestMethod]
		//TODO sth similar to DataRow
		public async Task PublishAsync_ForValidReportWithSeveralEntries_ReportsCorrectSfHealthInformation()
		{
			foreach(PublisherTestContext publisherContext in PublishTestManager.TestCases())
			{
				// Arrange.
				publisherContext.Configure();

				string healthyCheckName = "HealthyCheck";
				HealthReportEntry healthyEntry = new HealthReportEntry(
					status: HealthStatus.Healthy,
					description: "Healthy entry description",
					duration: TimeSpan.FromMilliseconds(12639),
					exception: null,
					data: null);

				string degradedCheckName = "DegradedCheck";
				HealthReportEntry degradedEntry = new HealthReportEntry(
					status: HealthStatus.Degraded,
					description: "Degraded entry description",
					duration: TimeSpan.FromMilliseconds(111),
					exception: null,
					data: null);

				string unhealthyCheckName = "UnhealthyCheck";
				HealthReportEntry unhealthyEntry = new HealthReportEntry(
					status: HealthStatus.Unhealthy,
					description: "Unhealthy entry description",
					duration: TimeSpan.FromMilliseconds(73),
					exception: new InvalidOperationException("Unhealthy check performed invalid operation."),
					data: null);

				HealthReport report = new HealthReportBuilder()
					.AddEntry(healthyCheckName, healthyEntry)
					.AddEntry(degradedCheckName, degradedEntry)
					.AddEntry(unhealthyCheckName, unhealthyEntry)
					.SetTotalDuration(TimeSpan.FromSeconds(72))
					.ToHealthReport();

				// Act.
				if (publisherContext.Publisher != null)
				{
					await publisherContext.Publisher.PublishAsync(report, cancellationToken: default);
				}
				else
				{
					Assert.Fail();
				}

				// Assert.
				SfHealthInformation? healthyInfo = publisherContext.ReportedSfHealthInformation(healthyCheckName);

				NullableAssert.IsNotNull(healthyInfo);
				NullableAssert.IsNotNull(publisherContext.Publisher);
				Assert.AreEqual(publisherContext.Publisher.HealthReportSourceId, healthyInfo.SourceId, "SourceId from healthy check is incorrect.");
				Assert.AreEqual(healthyCheckName, healthyInfo.Property, "Property from healthy check is incorrect.");
				Assert.AreEqual(SfHealthState.Ok, healthyInfo.HealthState, "HealthState from healthy check is incorrect.");
				StringAssert.Contains(healthyInfo.Description, healthyEntry.Description, "Description from healthy check is not included.");
				StringAssert.Contains(healthyInfo.Description, healthyEntry.Duration.ToString(), "Duration from healthy check is not included.");

				SfHealthInformation? degradedInfo = publisherContext.ReportedSfHealthInformation(degradedCheckName);
				NullableAssert.IsNotNull(degradedInfo);

				Assert.AreEqual(publisherContext.Publisher.HealthReportSourceId, degradedInfo.SourceId, "SourceId from degraded check is incorrect.");
				Assert.AreEqual(degradedCheckName, degradedInfo.Property, "Property from degraded check is incorrect.");
				Assert.AreEqual(SfHealthState.Warning, degradedInfo.HealthState, "HealthState from degraded check is incorrect.");
				StringAssert.Contains(degradedInfo.Description, degradedEntry.Description, "Description from degraded check is not included.");
				StringAssert.Contains(degradedInfo.Description, degradedEntry.Duration.ToString(), "Duration from degraded check is not included.");

				SfHealthInformation? unhealthyInfo = publisherContext.ReportedSfHealthInformation(unhealthyCheckName);
				NullableAssert.IsNotNull(unhealthyInfo);

				Assert.AreEqual(publisherContext.Publisher.HealthReportSourceId, unhealthyInfo.SourceId, "SourceId from unhealthy check is incorrect.");
				Assert.AreEqual(unhealthyCheckName, unhealthyInfo.Property, "Property from unhealthy check is incorrect.");
				Assert.AreEqual(SfHealthState.Error, unhealthyInfo.HealthState, "HealthState from unhealthy check is incorrect.");
				StringAssert.Contains(unhealthyInfo.Description, unhealthyEntry.Description, "Description from unhealthy check is not included.");
				StringAssert.Contains(unhealthyInfo.Description, unhealthyEntry.Duration.ToString(), "Duration from unhealthy check is not included.");
				StringAssert.Contains(unhealthyInfo.Description, unhealthyEntry.Exception?.GetType().ToString(), "Exception from unhealthy check is not included.");

				SfHealthInformation? summaryInfo = publisherContext.ReportedSfHealthInformation(ServiceFabricHealthCheckPublisher.HealthReportSummaryProperty);
				NullableAssert.IsNotNull(summaryInfo);

				Assert.AreEqual(publisherContext.Publisher.HealthReportSourceId, summaryInfo.SourceId, "SourceId from report is incorrect.");
				Assert.AreEqual(ServiceFabricHealthCheckPublisher.HealthReportSummaryProperty, summaryInfo.Property, "Property from report is incorrect.");
				Assert.AreEqual(SfHealthState.Error, summaryInfo.HealthState, "HealthState from report is incorrect.");
				StringAssert.Contains(summaryInfo.Description, report.TotalDuration.ToString(), "TotalDuration from report is not included.");
			}
		}

		private class PublishTestManager
		{
			public static List<PublisherTestContext> TestCases()
			{
				List<PublisherTestContext> cases = new();
				cases.Add(new StatelessSFPublisherTestContext());
				cases.Add(new StatefulSFPublisherTestContext());
				cases.Add(new RestPublisherTestContext());

				return cases;
			}
		}

		private class HealthReportBuilder
		{
			private Dictionary<string, HealthReportEntry> Entries { get; } = new Dictionary<string, HealthReportEntry>();

			private TimeSpan TotalDuration { get; set; }

			public static HealthReport EmptyHealthReport { get; } = new HealthReport(new Dictionary<string, HealthReportEntry>(), totalDuration: default);

			public HealthReportBuilder AddEntry(string healthCheckName, HealthStatus status)
			{
				HealthReportEntry entry = new HealthReportEntry(status, description: null, duration: default, exception: null, data: null);
				Entries.Add(healthCheckName, entry); // Health report shouldn't contain duplicate health check names.
				return this;
			}

			public HealthReportBuilder AddEntry(string healthCheckName, HealthReportEntry entry)
			{
				Entries.Add(healthCheckName, entry);
				return this;
			}

			public HealthReportBuilder SetTotalDuration(TimeSpan totalDuration)
			{
				TotalDuration = totalDuration;
				return this;
			}

			public HealthReport ToHealthReport() => new HealthReport(Entries, TotalDuration);
		}

		internal abstract class PublisherTestContext
		{
			public PublisherTestContext()
			{
				Configure();
			}

			public abstract void Configure(bool closed = false);

			public HealthCheckPublisher? Publisher { get; protected set; }

			public SfHealthInformation? ReportedSfHealthInformation(string propertyName) =>
				ReportedSfHealthInformatioStore.ContainsKey(propertyName)
					? ReportedSfHealthInformatioStore[propertyName]
					: null;

			public Dictionary<string, SfHealthInformation> TestGet() => ReportedSfHealthInformatioStore;


			protected readonly Dictionary<string, SfHealthInformation> ReportedSfHealthInformatioStore = new Dictionary<string, SfHealthInformation>();

			protected static ObjectPoolProvider ObjectPoolProvider { get; } = new DefaultObjectPoolProvider();
		}

		internal abstract class SFPublisherTestContext : PublisherTestContext
		{
			public Mock<IStatefulServicePartition>? MockStatefulServicePartition { get; protected set; }

			public Mock<IStatelessServicePartition>? MockStatelessServicePartition { get; protected set; }

			public Mock<IServicePartition>? MockServicePartition { get; protected set; }
		}

		internal class UnknownSFPublisherTestContext : PublisherTestContext
		{
			public UnknownSFPublisherTestContext() : base()
			{
			}

			public override void Configure(bool closed = false)
			{
				// Partition is not available by default.
				Accessor<IServicePartition> partitionAccessor = new Accessor<IServicePartition>(null);
				Publisher = new ServiceFabricHealthCheckPublisher(partitionAccessor, new NullLogger<ServiceFabricHealthCheckPublisher>(), ObjectPoolProvider);
			}
		}

		internal class StatefulSFPublisherTestContext : SFPublisherTestContext
		{
			public StatefulSFPublisherTestContext() : base()
			{
			}

			public override void Configure(bool closed = false)
			{
				MockStatefulServicePartition = new Mock<IStatefulServicePartition>();
				MockStatefulServicePartition.Setup(_ => _.ReportReplicaHealth(It.IsAny<SfHealthInformation>()))
					.Callback<SfHealthInformation>((Action<SfHealthInformation>)(info =>
					{
						if (closed)
						{
							throw new FabricObjectClosedException();
						}

						ReportedSfHealthInformatioStore[info.Property] = info;
					}));
				Accessor<IServicePartition> partitionAccessor = new Accessor<IServicePartition>(MockStatefulServicePartition.Object);
				Publisher = new ServiceFabricHealthCheckPublisher(partitionAccessor, new NullLogger<ServiceFabricHealthCheckPublisher>(), ObjectPoolProvider);
			}
		}

		internal class StatelessSFPublisherTestContext : SFPublisherTestContext
		{
			public StatelessSFPublisherTestContext() : base()
			{
			}

			public override void Configure(bool closed = false)
			{
				MockStatelessServicePartition = new Mock<IStatelessServicePartition>();
				MockStatelessServicePartition.Setup(_ => _.ReportInstanceHealth(It.IsAny<SfHealthInformation>()))
					.Callback<SfHealthInformation>(info =>
					{
						if (closed)
						{
							throw new FabricObjectClosedException();
						}

						ReportedSfHealthInformatioStore[info.Property] = info;
					});
				Accessor<IServicePartition> partitionAccessor = new Accessor<IServicePartition>(MockStatelessServicePartition.Object);
				Publisher = new ServiceFabricHealthCheckPublisher(partitionAccessor, new NullLogger<ServiceFabricHealthCheckPublisher>(), ObjectPoolProvider);
			}
		}

		internal class RestPublisherTestContext : SFPublisherTestContext
		{
			public Mock<IServiceFabricClient>? SFClient { get; protected set; } // TODO: move to RestTestContext

			public RestPublisherTestContext() : base()
			{
			}

			public override void Configure(bool closed = false)
			{
				SFClient = new Mock<IServiceFabricClient>();
				SFClient.Setup(_ => _.Nodes.ReportNodeHealthAsync(It.IsAny<NodeName>(),
					It.IsAny<SfcHealthInformation>(),
					It.IsAny<bool?>(),
					It.IsAny<long?>(),
					It.IsAny<CancellationToken>()))
					.Callback<NodeName, SfcHealthInformation, bool?, long?, CancellationToken>
					(
					(_, info, _, _, _) =>
					{
						if (closed)
						{
							throw new FabricObjectClosedException();
						}

						SfHealthState healthState = info.HealthState switch
						{
							sfcHealthState.Invalid => SfHealthState.Invalid,
							sfcHealthState.Ok => SfHealthState.Ok,
							sfcHealthState.Warning => SfHealthState.Warning,
							sfcHealthState.Error => SfHealthState.Error,
							sfcHealthState.Unknown => SfHealthState.Unknown,
							_ => throw new ArgumentException($"'{info.HealthState}' is not a valid health status."),
						};

						SfHealthInformation sfInfo = new(
							sourceId: info.SourceId,
							property: info.Property,
							healthState: healthState
							)
						{
							Description = info.Description,
						};

						ReportedSfHealthInformatioStore[info.Property] = sfInfo;
					});

				Publisher = new RestHealthCheckPublisher(new NullLogger<RestHealthCheckPublisher>(),
					SFClient.Object, ObjectPoolProvider, "mockNodeName");
			}
		}

	}
}
