// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Composables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests.Composables;

[TestClass]
public class HttpEndpointHealthCheckTests
{
	private static readonly ActivitySource s_activitySource = new(nameof(ObservableHealthCheckTests));

	[TestMethod]
	[DataRow(HttpStatusCode.OK, HealthStatus.Healthy)]
	[DataRow(HttpStatusCode.InternalServerError, HealthStatus.Unhealthy)]
	[DataRow(HttpStatusCode.NotFound, HealthStatus.Unhealthy)]
	[DataRow(HttpStatusCode.Forbidden, HealthStatus.Unhealthy)]
	[DataRow(HttpStatusCode.Unauthorized, HealthStatus.Unhealthy)]
	public async Task HttpEndpointHealthCheck_OkResponseChecker_ShouldReturnHealthy_WhenEndpointReturnsSuccessStatusCode(
		HttpStatusCode responseStatusCode,
		HealthStatus expectedHealthStatus)
	{
		HttpRequestMessage request = HealthCheckTestHelpers.GetHttpRequestMessageMock();
		HttpResponseMessage response = HealthCheckTestHelpers.GetHttpResponseMessageMock(responseStatusCode, string.Empty);
		Mock<IHttpClientFactory> httpClientFactoryMock = HealthCheckTestHelpers.GetHttpClientFactoryMock(response);

		HealthChecks.Composables.HttpEndpointHealthCheck healthCheck = new(
			httpClientFactoryMock.Object,
			() => request,
			(ctx, r, _) => HealthCheckComposablesExtensions.CheckResponseStatusCodeAsync(ctx, r),
			s_activitySource);

		CancellationTokenSource source = new();

		HealthCheckResult result = await healthCheck.CheckHealthAsync(
			HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
			source.Token);

		Assert.AreEqual(expectedHealthStatus, result.Status);
	}

	[TestMethod]
	[DataRow(HttpStatusCode.OK, HealthStatus.Healthy, new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest })]
	[DataRow(HttpStatusCode.BadRequest, HealthStatus.Healthy, new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest })]
	[DataRow(HttpStatusCode.InternalServerError, HealthStatus.Unhealthy, new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest })]
	[DataRow(HttpStatusCode.NotFound, HealthStatus.Unhealthy, new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest })]
	[DataRow(HttpStatusCode.Forbidden, HealthStatus.Unhealthy, new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest })]
	[DataRow(HttpStatusCode.Unauthorized, HealthStatus.Unhealthy, new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest })]
	public async Task HttpEndpointHealthCheck_OkResponseChecker_ShouldReturnHealthy_WhenEndpointReturnsAllowedStatusCode(
		HttpStatusCode responseStatusCode,
		HealthStatus expectedHealthStatus,
		HttpStatusCode[] allowedStatusCodes)
	{
		HttpRequestMessage request = HealthCheckTestHelpers.GetHttpRequestMessageMock();
		HttpResponseMessage response = HealthCheckTestHelpers.GetHttpResponseMessageMock(responseStatusCode, string.Empty);
		Mock<IHttpClientFactory> httpClientFactoryMock = HealthCheckTestHelpers.GetHttpClientFactoryMock(response);
		HealthCheckParameters parameters = new();

		HealthChecks.Composables.HttpEndpointHealthCheck healthCheck = new(
			httpClientFactoryMock.Object,
			() => request,
			(ctx, r, _) => HealthCheckComposablesExtensions.CheckResponseStatusCodeAsync(ctx, r, allowedStatusCodes),
			s_activitySource);

		CancellationTokenSource source = new();

		HealthCheckResult result = await healthCheck.CheckHealthAsync(
			HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
			source.Token);

		Assert.AreEqual(expectedHealthStatus, result.Status);
	}

	[TestMethod]
	[DataRow("expected body", "expected body", HealthStatus.Healthy)]
	[DataRow("expected body", "returned body", HealthStatus.Unhealthy)]
	public async Task HttpEndpointHealthCheck_CustomResponseChecker_ShouldReturnHealthy_WhenEndpointReturnsSuccessStatusCode(
		string expectedBody,
		string responseBody,
		HealthStatus expectedHealthStatus)
	{
		HttpRequestMessage request = HealthCheckTestHelpers.GetHttpRequestMessageMock();
		HttpResponseMessage response = HealthCheckTestHelpers.GetHttpResponseMessageMock(HttpStatusCode.OK, responseBody);
		Mock<IHttpClientFactory> httpClientFactoryMock = HealthCheckTestHelpers.GetHttpClientFactoryMock(response);

		HealthChecks.Composables.HttpEndpointHealthCheck healthCheck = new(
			httpClientFactoryMock.Object,
			() => request,
			new CustomHealthCheckResponseChecker(expectedBody).CheckResponseAsync,
			s_activitySource);

		CancellationTokenSource source = new();

		HealthCheckResult result = await healthCheck.CheckHealthAsync(
			HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
			source.Token);

		Assert.AreEqual(expectedHealthStatus, result.Status);
	}
}

internal class CustomHealthCheckResponseChecker
{
	private string m_expectedResponse = string.Empty;

	public CustomHealthCheckResponseChecker(string expectedResponse)
	{
		m_expectedResponse = expectedResponse;
	}

	public async Task<HealthCheckResult> CheckResponseAsync(
		HealthCheckContext context,
		HttpResponseMessage response,
		CancellationToken cancellationToken)
	{
#if NET5_0_OR_GREATER
		string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
#else
		string responseContent = await response.Content.ReadAsStringAsync();
#endif

		return responseContent.Equals(m_expectedResponse, StringComparison.InvariantCulture)
			? HealthCheckResult.Healthy()
			: HealthCheckResult.Unhealthy();
	}
}

