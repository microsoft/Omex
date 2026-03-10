// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Composables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests.Composables;

[TestClass]
public class ObservableHealthCheckTests
{
	private static readonly ActivitySource s_activitySource = new(nameof(ObservableHealthCheckTests));

	private static readonly ILogger s_logger = NullLogger.Instance;

	[TestMethod]
	[DataRow(HealthStatus.Healthy, HealthStatus.Degraded, HealthStatus.Healthy)]
	[DataRow(HealthStatus.Healthy, HealthStatus.Unhealthy, HealthStatus.Healthy)]
	[DataRow(HealthStatus.Unhealthy, HealthStatus.Degraded, HealthStatus.Degraded)]
	[DataRow(HealthStatus.Unhealthy, HealthStatus.Unhealthy, HealthStatus.Unhealthy)]
	[DataRow(HealthStatus.Degraded, HealthStatus.Degraded, HealthStatus.Degraded)]
	[DataRow(HealthStatus.Degraded, HealthStatus.Unhealthy, HealthStatus.Unhealthy)]
	public async Task MonitorHealthCheck_ShouldReturnGivenState_WhenHealthCheckHasGivenStatus(
		HealthStatus healthStatus,
		HealthStatus registeredFailureStatus,
		HealthStatus expectedStatus)
	{
		Mock<IHealthCheck> healthCheckMock = new();
		healthCheckMock.Setup(hc => hc.CheckHealthAsync(It.IsAny<HealthCheckContext>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new HealthCheckResult(healthStatus));

		ObservableHealthCheck monitorHealthCheck = new(new(), healthCheckMock.Object, s_activitySource, s_logger);

		CancellationTokenSource source = new();

		HealthCheckResult result = await monitorHealthCheck.CheckHealthAsync(
			HealthCheckTestHelpers.GetHealthCheckContext(monitorHealthCheck, registeredFailureStatus),
			source.Token);

		Assert.AreEqual(expectedStatus, result.Status);
	}

	[TestMethod]
	public async Task MonitorHealthCheck_ShouldReturnUnhealthy_WhenHealthCheckThrowsException()
	{
		HealthCheckTestHelpers.InMemoryLogger logger = HealthCheckTestHelpers.GetLoggerInstance();

		const string ExceptionMessage = "Exception message";
		Mock<IHealthCheck> healthCheckMock = new();
		healthCheckMock.Setup(hc => hc.CheckHealthAsync(It.IsAny<HealthCheckContext>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException(ExceptionMessage));

		ObservableHealthCheck monitorHealthCheck = new(new(), healthCheckMock.Object, s_activitySource, logger);

		CancellationTokenSource source = new();
		HealthCheckContext context = HealthCheckTestHelpers.GetHealthCheckContext(monitorHealthCheck);

		HealthCheckResult result = await monitorHealthCheck.CheckHealthAsync(
			context,
			source.Token);

		Assert.AreEqual(context.Registration.FailureStatus, result.Status);
	}
}
