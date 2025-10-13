// Copyright (C) Microsoft Corporation. All rights reserved.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Composables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests.Composables;

[TestClass]
public class StartupHealthCheckTests
{
	private static readonly ActivitySource s_activitySource = new(nameof(StartupHealthCheckTests));

	private static readonly ILogger s_logger = NullLogger.Instance;

	[TestMethod]
	[DataRow(HealthStatus.Healthy)]
	[DataRow(HealthStatus.Unhealthy)]
	[DataRow(HealthStatus.Degraded)]
	public async Task StartupHealthCheck_ShouldReturnGivenState_WhenHealthCheckHasGivenStatus(HealthStatus healthStatus)
	{
		IMemoryCache memoryCacheMock = new MemoryCache(new MemoryCacheOptions());
		Mock<IHealthCheck> healthCheckMock = new();
		healthCheckMock.Setup(hc => hc.CheckHealthAsync(It.IsAny<HealthCheckContext>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new HealthCheckResult(healthStatus));

		StartupHealthCheck monitorHealthCheck = new(healthCheckMock.Object, memoryCacheMock);

		CancellationTokenSource source = new();

		HealthCheckResult result = await monitorHealthCheck.CheckHealthAsync(
			HealthCheckTestHelpers.GetHealthCheckContext(monitorHealthCheck),
			source.Token);

		Assert.AreEqual(healthStatus, result.Status);
	}

	[TestMethod]
	public async Task CheckHealthAsync_WithThreeRun_ShouldReturnHealthy()
	{
		IHealthCheck wrappedHealthCheck = new FailingThreeTimeHealthCheck();

		IMemoryCache memoryCacheMock = new MemoryCache(new MemoryCacheOptions());
		CancellationTokenSource source = new();
		StartupHealthCheck healthCheck = new(wrappedHealthCheck, memoryCacheMock);

		for (int i = 0; i < 3; i++)
		{
			_ = await healthCheck.CheckHealthAsync(HealthCheckTestHelpers.GetHealthCheckContext(healthCheck), source.Token);
		}

		HealthCheckResult response = await healthCheck.CheckHealthAsync(
			HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
			source.Token);

		HealthCheckResult responseAfterFirstOk = await healthCheck.CheckHealthAsync(
			HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
			source.Token);

		Assert.AreEqual(HealthStatus.Healthy, response.Status);
		Assert.AreEqual(HealthStatus.Healthy, responseAfterFirstOk.Status);
	}
}

internal class FailingThreeTimeHealthCheck : IHealthCheck
{
	private int m_count = 0;

	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		if (m_count < 2)
		{
			Interlocked.Increment(ref m_count);
			return await Task.FromResult(new HealthCheckResult(HealthStatus.Unhealthy));
		}

		return await Task.FromResult(new HealthCheckResult(HealthStatus.Healthy));
	}
}
