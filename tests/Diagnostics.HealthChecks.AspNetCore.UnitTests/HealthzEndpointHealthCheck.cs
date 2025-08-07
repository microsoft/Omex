// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Diagnostics.HealthChecks.AspNetCore;
using Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests.Composables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.OMEX.ClusterDiagnostics.Service.Tests.HealthChecks;

[TestClass]
public class HealthzEndpointHealthCheckTests
{
	private static ILogger<T> GetLogger<T>() => new NullLogger<T>();

	[TestInitialize]
	public void Setup() => Environment.SetEnvironmentVariable($"Fabric_Endpoint_{nameof(EndpointLivenessHealthCheck)}", "1234");

	[TestMethod]
	[DataRow(HttpStatusCode.OK, HealthStatus.Healthy)]
	[DataRow(HttpStatusCode.InternalServerError, HealthStatus.Unhealthy)]
	[DataRow(HttpStatusCode.NotFound, HealthStatus.Unhealthy)]
	[DataRow(HttpStatusCode.Unauthorized, HealthStatus.Unhealthy)]
	[DataRow(HttpStatusCode.Forbidden, HealthStatus.Unhealthy)]
	public async Task HealthzEndpointHttpHealthCheck_ReturnsExpectedStatus(HttpStatusCode returnedStatusCode, HealthStatus expectedHealthStatus)
	{
		EndpointLivenessHealthCheckParameters healthCheckParameters = new(
			nameof(EndpointLivenessHealthCheck),
			$"{nameof(EndpointLivenessHealthCheck)}_HttpClient",
			"healthz");

		HealthCheckTestHelpers.SetLocalServiceInfo();
		Mock<IHttpClientFactory> clientFactory = HealthCheckTestHelpers.GetHttpClientFactoryMock(
			HealthCheckTestHelpers.GetHttpResponseMessageMock(returnedStatusCode, message: string.Empty));

		ActivitySource activitySourceMock = new(nameof(EndpointLivenessHealthCheck));

		IHealthCheck healthCheck = new EndpointLivenessHealthCheck(
			clientFactory.Object,
			activitySourceMock,
			GetLogger<EndpointLivenessHealthCheck>(),
			healthCheckParameters);

		CancellationTokenSource source = new();

		HealthCheckResult healthCheckResult = await healthCheck.CheckHealthAsync(
			HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
			source.Token);

		Assert.AreEqual(expectedHealthStatus, healthCheckResult.Status);
	}

	[TestMethod]
	[DataRow(new[] { HttpStatusCode.OK }, HttpStatusCode.OK, HealthStatus.Healthy)]
	[DataRow(new[] { HttpStatusCode.Unauthorized }, HttpStatusCode.Unauthorized, HealthStatus.Healthy)]
	[DataRow(new[] { HttpStatusCode.NotFound }, HttpStatusCode.OK, HealthStatus.Unhealthy)]
	[DataRow(new[] { HttpStatusCode.NotFound }, HttpStatusCode.NotFound, HealthStatus.Healthy)]
	[DataRow(new[] { HttpStatusCode.NotFound, HttpStatusCode.OK }, HttpStatusCode.NotFound, HealthStatus.Healthy)]
	[DataRow(new[] { HttpStatusCode.NotFound, HttpStatusCode.OK }, HttpStatusCode.Unauthorized, HealthStatus.Unhealthy)]
	public async Task HealthzEndpointHttpHealthCheckWithExpectedStatusCodes_ReturnsExpectedStatus(HttpStatusCode[] expectedStatusCode, HttpStatusCode returnedStatusCode, HealthStatus expectedHealthStatus)
	{
		EndpointLivenessHealthCheckParameters healthCheckParameters = new(
			nameof(EndpointLivenessHealthCheck),
			$"{nameof(EndpointLivenessHealthCheck)}_HttpClient",
			"healthz",
			expectedStatusCode);

		HealthCheckTestHelpers.SetLocalServiceInfo();
		Mock<IHttpClientFactory> clientFactory = HealthCheckTestHelpers.GetHttpClientFactoryMock(
			HealthCheckTestHelpers.GetHttpResponseMessageMock(returnedStatusCode, message: string.Empty));

		ActivitySource activitySourceMock = new(nameof(EndpointLivenessHealthCheck));

		EndpointLivenessHealthCheck healthCheck = new(
			clientFactory.Object,
			activitySourceMock,
			GetLogger<EndpointLivenessHealthCheck>(),
			healthCheckParameters);

		CancellationTokenSource source = new();

		HealthCheckResult healthCheckResult = await healthCheck.CheckHealthAsync(
			HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
			source.Token);

		Assert.AreEqual(expectedHealthStatus, healthCheckResult.Status);
	}


	[TestMethod]
	[DataRow(HttpStatusCode.OK, HealthStatus.Healthy)]
	[DataRow(HttpStatusCode.InternalServerError, HealthStatus.Unhealthy)]
	[DataRow(HttpStatusCode.NotFound, HealthStatus.Unhealthy)]
	[DataRow(HttpStatusCode.Unauthorized, HealthStatus.Unhealthy)]
	[DataRow(HttpStatusCode.Forbidden, HealthStatus.Unhealthy)]
	public async Task HealthzEndpointHttpHealthCheck_ReturnsCorrectErrorMessageAndException(HttpStatusCode returnedStatusCode, HealthStatus expectedHealthStatus)
	{
		EndpointLivenessHealthCheckParameters healthCheckParameters = new(
			nameof(EndpointLivenessHealthCheck),
			$"{nameof(EndpointLivenessHealthCheck)}_HttpClient",
			"healthz");

		HealthCheckTestHelpers.SetLocalServiceInfo();
		Mock<IHttpClientFactory> clientFactory = HealthCheckTestHelpers.GetHttpClientFactoryMock(
			HealthCheckTestHelpers.GetHttpResponseMessageMock(returnedStatusCode, message: string.Empty),
			shouldThrowException: true);

		ActivitySource activitySourceMock = new(nameof(EndpointLivenessHealthCheck));

		IHealthCheck healthCheck = new EndpointLivenessHealthCheck(
			clientFactory.Object,
			activitySourceMock,
			GetLogger<EndpointLivenessHealthCheck>(),
			healthCheckParameters);

		CancellationTokenSource source = new();

		HealthCheckResult healthCheckResult = await healthCheck.CheckHealthAsync(
			HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
			source.Token);

		Assert.AreEqual(expectedHealthStatus, healthCheckResult.Status);

		if (expectedHealthStatus != HealthStatus.Healthy)
		{
			Assert.IsTrue(healthCheckResult.Description?.Contains(returnedStatusCode.ToString(), StringComparison.InvariantCulture));
		}
	}

	[TestMethod]
	public async Task HealthzEndpointHttpHealthCheck_ReturnsCorrectResponseWhenHttpExpected()
	{
		EndpointLivenessHealthCheckParameters healthCheckParameters = new(
			nameof(EndpointLivenessHealthCheck),
			$"{nameof(EndpointLivenessHealthCheck)}_HttpClient",
			"healthz");

		HealthCheckTestHelpers.SetLocalServiceInfo();
		Mock<IHttpClientFactory> httpOnlyClientFactory = HealthCheckTestHelpers.GetHttpClientFactoryMock(
			HealthCheckTestHelpers.GetHttpResponseMessageMock(HttpStatusCode.OK, "Response is OK."),
			shouldThrowException: false,
			requestMatch: request => request.RequestUri?.Scheme == Uri.UriSchemeHttp);

		ActivitySource activitySourceMock = new(nameof(EndpointLivenessHealthCheck));

		IHealthCheck healthCheck = new EndpointLivenessHealthCheck(
			httpOnlyClientFactory.Object,
			activitySourceMock,
			GetLogger<EndpointLivenessHealthCheck>(),
			healthCheckParameters);

		CancellationTokenSource source = new();

		HealthCheckResult healthCheckResult = await healthCheck.CheckHealthAsync(
			HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
			source.Token);

		Assert.AreEqual(HealthStatus.Healthy, healthCheckResult.Status);
	}

	[TestMethod]
	public async Task HealthzEndpointHttpHealthCheck_ReturnsErrorWhenHttpsExpected()
	{
		EndpointLivenessHealthCheckParameters healthCheckParameters = new(
			nameof(EndpointLivenessHealthCheck),
			$"{nameof(EndpointLivenessHealthCheck)}_HttpClient",
			"healthz");

		HealthCheckTestHelpers.SetLocalServiceInfo();
		Mock<IHttpClientFactory> httpOnlyClientFactory = HealthCheckTestHelpers.GetHttpClientFactoryMock(
			HealthCheckTestHelpers.GetHttpResponseMessageMock(HttpStatusCode.OK, "Response is OK."),
			shouldThrowException: false,
			requestMatch: request => request.RequestUri?.Scheme == Uri.UriSchemeHttps);

		ActivitySource activitySourceMock = new(nameof(EndpointLivenessHealthCheck));

		IHealthCheck healthCheck = new EndpointLivenessHealthCheck(
			httpOnlyClientFactory.Object,
			activitySourceMock,
			GetLogger<EndpointLivenessHealthCheck>(),
			healthCheckParameters);

		CancellationTokenSource source = new();

		HealthCheckResult healthCheckResult = await healthCheck.CheckHealthAsync(
			HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
			source.Token);

		Assert.AreEqual(HealthStatus.Unhealthy, healthCheckResult.Status);
	}

	[TestMethod]
	public async Task HealthzEndpointHttpHealthCheck_ReturnsCorrectResponseWhenHttpsExpected()
	{
		EndpointLivenessHealthCheckParameters healthCheckParameters = new(
			nameof(EndpointLivenessHealthCheck),
			$"{nameof(EndpointLivenessHealthCheck)}_HttpClient",
			"healthz",
			uriScheme: Uri.UriSchemeHttps);

		HealthCheckTestHelpers.SetLocalServiceInfo();
		Mock<IHttpClientFactory> httpOnlyClientFactory = HealthCheckTestHelpers.GetHttpClientFactoryMock(
			HealthCheckTestHelpers.GetHttpResponseMessageMock(HttpStatusCode.OK, "Response is OK."),
			shouldThrowException: false,
			requestMatch: request => request.RequestUri?.Scheme == Uri.UriSchemeHttps);

		ActivitySource activitySourceMock = new(nameof(EndpointLivenessHealthCheck));

		IHealthCheck healthCheck = new EndpointLivenessHealthCheck(
			httpOnlyClientFactory.Object,
			activitySourceMock,
			GetLogger<EndpointLivenessHealthCheck>(),
			healthCheckParameters);

		CancellationTokenSource source = new();

		HealthCheckResult healthCheckResult = await healthCheck.CheckHealthAsync(
			HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
			source.Token);

		Assert.AreEqual(HealthStatus.Healthy, healthCheckResult.Status);
	}

	[TestMethod]
	public async Task HealthzEndpointHttpHealthCheck_ReturnsErrorWhenHttpExpected()
	{
		EndpointLivenessHealthCheckParameters healthCheckParameters = new(
			nameof(EndpointLivenessHealthCheck),
			$"{nameof(EndpointLivenessHealthCheck)}_HttpClient",
			"healthz",
			null,
			Uri.UriSchemeHttps);

		HealthCheckTestHelpers.SetLocalServiceInfo();
		Mock<IHttpClientFactory> httpOnlyClientFactory = HealthCheckTestHelpers.GetHttpClientFactoryMock(
			HealthCheckTestHelpers.GetHttpResponseMessageMock(HttpStatusCode.OK, "Response is OK."),
			shouldThrowException: true,
			requestMatch: request => request.RequestUri?.Scheme == Uri.UriSchemeHttp);

		ActivitySource activitySourceMock = new(nameof(EndpointLivenessHealthCheck));

		IHealthCheck healthCheck = new EndpointLivenessHealthCheck(
			httpOnlyClientFactory.Object,
			activitySourceMock,
			GetLogger<EndpointLivenessHealthCheck>(),
			healthCheckParameters);

		CancellationTokenSource source = new();

		HealthCheckResult healthCheckResult = await healthCheck.CheckHealthAsync(
			HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
			source.Token);

		Assert.AreEqual(HealthStatus.Healthy, healthCheckResult.Status);
	}
}

