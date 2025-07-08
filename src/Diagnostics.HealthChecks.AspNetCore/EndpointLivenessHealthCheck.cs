// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Composables;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.AspNetCore;

/// <summary>
/// This health check will perform a liveness call to the endpoint identified by the given parameters.
/// </summary>
internal class EndpointLivenessHealthCheck : IHealthCheck
{
	private readonly EndpointLivenessHealthCheckParameters m_parameters;
	private readonly IHealthCheck m_healthCheck;

	/// <summary>
	/// Constructor.
	/// </summary>
	public EndpointLivenessHealthCheck(
		IHttpClientFactory httpClientFactory,
		ActivitySource activitySource,
		ILogger<EndpointLivenessHealthCheck> logger,
		EndpointLivenessHealthCheckParameters parameters)
	{
		m_parameters = parameters;
		m_healthCheck = HealthCheckComposablesExtensions.CreateLivenessHttpHealthCheck(
			CreateHttpRequestMessage,
			activitySource,
			httpClientFactory,
			expectedStatus: parameters.ExpectedStatus,
			httpClientName: parameters.HttpClientLogicalName)
				.AsObservableHealthCheck(activitySource, logger, parameters: parameters);
	}

	/// <inheritdoc />
	public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
		m_healthCheck.CheckHealthAsync(context, cancellationToken);

	private HttpRequestMessage CreateHttpRequestMessage()
	{
		int port = SfConfigurationProvider.GetEndpointPort(m_parameters.EndpointName);
		UriBuilder uriBuilder = new(m_parameters.UriScheme, m_parameters.Host, port, m_parameters.EndpointRelativeUri);
		return new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
	}
}
