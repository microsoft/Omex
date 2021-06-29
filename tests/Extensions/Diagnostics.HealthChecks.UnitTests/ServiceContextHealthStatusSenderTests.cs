// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Fabric.Health;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	internal class ServiceContextHealthStatusSenderTests
	{
		[TestMethod]
		public async Task PublishAsync_WhenPartitionIsNotAvailable_ReturnsGracefully()
		{
			// Arrange.
			ServiceContextHealthStatusSender senderWithoutPartition = CreateSender(null);

			// Act.
			await InializeAnsSendHealthAsync(senderWithoutPartition);

			// Assert.
			// If we are here, PublishAsync didn't explode.
		}

		[TestMethod]
		public async Task PublishAsync_WhenStatefulPartitionIsClosed_ReturnsGracefully()
		{
			// Arrange.
			ServiceContextHealthStatusSender sender = CreateSenderForStateful(closed: true).Sender;

			// Act.
			await InializeAnsSendHealthAsync(sender);

			// Assert.
			// If we are here, PublishAsync didn't explode.
		}

		[TestMethod]
		public async Task PublishAsync_WhenStatelessPartitionIsClosed_ReturnsGracefully()
		{
			// Arrange.
			ServiceContextHealthStatusSender sender = CreateSenderForStateless(closed: true).Sender;

			// Act.
			await InializeAnsSendHealthAsync(sender);

			// Assert.
			// If we are here, PublishAsync didn't explode.
		}

		[TestMethod]
		public async Task PublishAsync_ForStatefulServicePartition_ReportsReplicaHealth()
		{
			// Arrange.
			SenderContext<IStatefulServicePartition> context = CreateSenderForStateful();

			// Act.
			await InializeAnsSendHealthAsync(context.Sender);

			// Assert.
			context.PartitionMock.Verify(p => p.ReportReplicaHealth(It.IsAny<HealthInformation>()));
		}

		[TestMethod]
		public async Task PublishAsync_ForStatelessServicePartition_ReportsInstanceHealth()
		{
			// Arrange.
			SenderContext<IStatelessServicePartition> context = CreateSenderForStateless();

			// Act.
			await InializeAnsSendHealthAsync(context.Sender);

			// Assert.
			context.PartitionMock.Verify(p => p.ReportInstanceHealth(It.IsAny<HealthInformation>()));
		}

		[TestMethod]
		public async Task PublishAsync_ForUnknownPartitionType_ThrowsArgumentException()
		{
			// Arrange.
			IServicePartition partition = new Mock<IServicePartition>().Object;
			ServiceContextHealthStatusSender sender = CreateSender(partition);

			// Act.
			Task act() => sender.IntializeAsync(default);

			// Assert.
			ArgumentException actualException = await Assert.ThrowsExceptionAsync<ArgumentException>(act);
			StringAssert.Contains(actualException.Message, partition.GetType().ToString());
		}

		[DataTestMethod]
		[DynamicData(nameof(StatusSenders), DynamicDataSourceType.Method)]
		public async Task SendStatusAsync_WhenUnsupportedStateUsed_Trows(SenderContext context)
		{
			await context.Sender.IntializeAsync(default);
			HealthStatus unsupportedHealthStatus = (HealthStatus)int.MinValue;

			await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
				context.Sender.SendStatusAsync("HealthCheckName", unsupportedHealthStatus, "SomeDescription", default));
		}

		[DataTestMethod]
		[DynamicData(nameof(StatusSenders), DynamicDataSourceType.Method)]
		public async Task SendStatusAsync_ReportsHealthCorrectly(SenderContext context)
		{
			(string name, string description, HealthStatus status, HealthState expected)[] checks = new[]
			{
				("HealthyCheck", "HealthDescription", HealthStatus.Healthy, HealthState.Ok),
				("DegradedCheck", "DegradedDescription", HealthStatus.Degraded, HealthState.Warning),
				("UnhealthyCheck", "UnhealthyDescription", HealthStatus.Unhealthy, HealthState.Error),
			};

			await context.Sender.IntializeAsync(default);

			foreach ((string name, string description, HealthStatus status, _) in checks)
			{
				await context.Sender.SendStatusAsync(name, status, description, default);
			}

			foreach ((string name, string description, _, HealthState expected) in checks)
			{
				Assert.IsTrue(context.ReportedState.TryGetValue(name, out HealthInformation? result));
				Assert.IsNotNull(result);
				Assert.AreEqual(name, result.HealthReportId);
				Assert.AreEqual(description, result.Description);
				Assert.AreEqual(expected, result.HealthState);
			}
		}

		[DataTestMethod]
		[DynamicData(nameof(StatusSenders), DynamicDataSourceType.Method)]
		public async Task SendStatusAsync_WhenNotInitialized_Throws(SenderContext context)
		{
			await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
				context.Sender.SendStatusAsync("HealthCheckName", HealthStatus.Healthy, "SomeDescription", default));
		}

		private async Task InializeAnsSendHealthAsync(ServiceContextHealthStatusSender sender)
		{
			await sender.IntializeAsync(default);
			await sender.SendStatusAsync("SomeCheck", HealthStatus.Healthy, "DumyDescription", default);
		}

		public static IEnumerable<object[]> StatusSenders()
		{
			yield return new object[] { CreateSenderForStateful() };
			yield return new object[] { CreateSenderForStateless() };
		}

		private static SenderContext<IStatefulServicePartition> CreateSenderForStateful(bool closed = false)
		{
			Dictionary<string, HealthInformation> reportedState = new();
			Mock<IStatefulServicePartition> partitionMock = new();
			partitionMock.Setup(_ => _.ReportReplicaHealth(It.IsAny<HealthInformation>()))
				.Callback((Action<HealthInformation>)(info =>
				{
					if (closed)
					{
						throw new FabricObjectClosedException();
					}

					reportedState[info.Property] = info;
				}));
			
			return new SenderContext<IStatefulServicePartition>(partitionMock, reportedState);
		}

		private static SenderContext<IStatelessServicePartition> CreateSenderForStateless(bool closed = false)
		{
			Dictionary<string, HealthInformation> reportedState = new();
			Mock<IStatelessServicePartition> partitionMock = new();
			partitionMock.Setup(_ => _.ReportInstanceHealth(It.IsAny<HealthInformation>()))
				.Callback<HealthInformation>(info =>
				{
					if (closed)
					{
						throw new FabricObjectClosedException();
					}

					reportedState[info.Property] = info;
				});

			return new SenderContext<IStatelessServicePartition>(partitionMock, reportedState);
		}

		private static ServiceContextHealthStatusSender CreateSender(IServicePartition? partition) =>
			new(new Accessor<IServicePartition>(partition), new NullLogger<ServiceContextHealthStatusSender>());


		internal record SenderContext(
			ServiceContextHealthStatusSender Sender,
			IDictionary<string, HealthInformation> ReportedState);

		internal record SenderContext<TPartition>(
			Mock<TPartition> PartitionMock,
			IDictionary<string, HealthInformation> ReportedState)
			: SenderContext(CreateSender(PartitionMock.Object), ReportedState)
				where TPartition : class, IServicePartition;
	}
}
