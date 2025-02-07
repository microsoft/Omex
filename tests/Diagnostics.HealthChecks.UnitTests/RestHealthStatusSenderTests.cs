// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions;
using Microsoft.ServiceFabric.Client;
using Microsoft.ServiceFabric.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class RestHealthStatusSenderTests
	{
		[TestMethod]
		public async Task IntializeAsync_CreatesClientOnce()
		{
			SenderInfo context = GetRestHealthStatusSender();

			await context.Sender.IntializeAsync(default);
			context.ClientWrapperMock.Verify(w => w.GetAsync(It.IsAny<CancellationToken>()), Times.Once, "SF Client created during initialization");

			await context.Sender.IntializeAsync(default);
			context.ClientWrapperMock.Verify(w => w.GetAsync(It.IsAny<CancellationToken>()), Times.Once, "SF Client should not be created on second call");
		}

		[TestMethod]
		public async Task SendStatusAsync_WhenNotInitialized_Throws()
		{
			SenderInfo context = GetRestHealthStatusSender();

			await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
				context.Sender.SendStatusAsync("HealthCheckName", HealthStatus.Healthy, "SomeDescription", default));
		}

		[TestMethod]
		public async Task SendStatusAsync_WhenUnsupportedStateUsed_Trows()
		{
			SenderInfo context = GetRestHealthStatusSender();
			HealthStatus unsupportedHealthStatus = (HealthStatus)int.MinValue;

			await context.Sender.IntializeAsync(default);
			await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
				context.Sender.SendStatusAsync("HealthCheckName", unsupportedHealthStatus, "SomeDescription", default));
		}

		[TestMethod]
		public async Task SendStatusAsync_ReportsHealthCorrectly()
		{
			(string name, string description, HealthStatus status, HealthState expected)[] checks = new[]
			{
				("HealthyCheck", "HealthDescription", HealthStatus.Healthy, HealthState.Ok),
				("DegradedCheck", "DegradedDescription", HealthStatus.Degraded, HealthState.Warning),
				("UnhealthyCheck", "UnhealthyDescription", HealthStatus.Unhealthy, HealthState.Error),
			};

			SenderInfo context = GetRestHealthStatusSender();
			await context.Sender.IntializeAsync(default);

			foreach ((string name, string description, HealthStatus status, _) in checks)
			{
				await context.Sender.SendStatusAsync(name, status, description, default);
			}

			foreach ((string name, string description, _, HealthState expected) in checks)
			{
				Assert.IsTrue(context.ReportedState.TryGetValue(name, out HealthInformation? result));
				Assert.IsNotNull(result);
				Assert.AreEqual(name, result.Property);
				Assert.AreEqual(description, result.Description);
				Assert.AreEqual(expected, result.HealthState);
			}
		}

		private SenderInfo GetRestHealthStatusSender()
		{
			Dictionary<string, HealthInformation> reportedState = new();

			Mock<IServiceFabricClient> sfClient = new();
			sfClient.Setup(_ => _.Nodes.ReportNodeHealthAsync(It.IsAny<NodeName>(),
				It.IsAny<HealthInformation>(),
				It.IsAny<bool?>(),
				It.IsAny<long?>(),
				It.IsAny<CancellationToken>()))
				.Callback<NodeName, HealthInformation, bool?, long?, CancellationToken>((_, info, _, _, _) => reportedState[info.Property] = info);

			Mock<IServiceFabricClientWrapper> sfClientWrapper = new();
			sfClientWrapper.Setup(w => w.GetAsync(It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(sfClient.Object));

			Mock<IExecutionContext> mockContext = new();
			mockContext.SetupGet(c => c.NodeName).Returns("TestNodeName");

			RestHealthStatusSender sender = new(sfClientWrapper.Object, mockContext.Object);

			return new SenderInfo(sender, sfClientWrapper, reportedState);
		}

		private record SenderInfo(
			RestHealthStatusSender Sender,
			Mock<IServiceFabricClientWrapper> ClientWrapperMock,
			IDictionary<string, HealthInformation> ReportedState);
	}
}
