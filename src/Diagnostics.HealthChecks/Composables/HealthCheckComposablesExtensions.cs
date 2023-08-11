// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Composables;

/// <summary>
/// Extension methods for health check decorators configuration.
/// </summary>
public static class HealthCheckComposablesExtensions
{
	/// <summary>
	/// Makes the Health check executable only at startup time, adding also observability to it.
	/// If the consumer health check of this extension method does not want automatic observability,
	/// the developer is encouraged to wrap it directly around the <see cref="StartupHealthCheck"/> class.
	/// </summary>
	/// <param name="healthCheck">The health check.</param>
	/// <param name="cache">The memory cache. It will be used to memorise the latest health check result.</param>
	/// <param name="activitySource">The activity source.</param>
	/// <param name="logger">The logger.</param>
	/// <param name="parameters">The health check parameters, set at DI registration time.</param>
	/// <returns>The health check executable only at deploy time.</returns>
	public static IHealthCheck AsObservableStartupHealthCheck(
		this IHealthCheck healthCheck,
		IMemoryCache cache,
		ActivitySource activitySource,
		ILogger logger,
		HealthCheckParameters? parameters = null)
	{
		IHealthCheck startupHealthCheck = new StartupHealthCheck(healthCheck, cache);
		return new ObservableHealthCheck(parameters ?? new(), startupHealthCheck, activitySource, logger);
	}

	/// <summary>
	/// Applies the default error handling and activity around the current health check.
	/// </summary>
	/// The instance of the Activity marker. By selecting the implementor, the health check will determine how
	/// the related endpoint will process the health check request.
	/// <param name="healthCheck">The health check.</param>
	/// <param name="activitySource">The activity source.</param>
	/// <param name="logger">The logger.</param>
	/// <param name="parameters">The health check parameters, set at DI registration time.</param>
	/// <returns>The health check with the default configuration and error handling.</returns>
	public static IHealthCheck AsObservableHealthCheck(
		this IHealthCheck healthCheck,
		ActivitySource activitySource,
		ILogger logger,
		HealthCheckParameters? parameters = null) =>
		new ObservableHealthCheck(parameters ?? new(), healthCheck, activitySource, logger);

	/// <summary>
	/// Creates a new HTTP endpoint checker health check.
	/// </summary>
	/// <param name="httpClientFactory">The http client factory.</param>
	/// <param name="requestBuilder">The request builder.</param>
	/// <param name="healthCheckResponseChecker">The response checker instance.</param>
	/// <param name="activitySource">The activity source.</param>
	/// <param name="httpClientName">The HTTP Client name, if a client with that name was registered in particular.</param>
	/// <param name="activityMarker">Marks the activity with the required baggage items.</param>
	/// <returns>The HTTP endpoint checker health check.</returns>
	public static IHealthCheck CreateHttpHealthCheck(
		Func<HttpRequestMessage> requestBuilder,
		Func<HealthCheckContext, HttpResponseMessage, CancellationToken, Task<HealthCheckResult>> healthCheckResponseChecker,
		ActivitySource activitySource,
		IHttpClientFactory httpClientFactory,
		string? httpClientName = "",
		Action<Activity?>? activityMarker = null) =>
			new HttpEndpointHealthCheck(
				httpClientFactory,
				requestBuilder,
				healthCheckResponseChecker,
				activitySource,
				httpClientName: httpClientName,
				activityMarker: activityMarker);

	/// <summary>
	/// Checks whether the response from the endpoint is successful or not by checking its HTTP Status Code.
	/// </summary>
	/// <param name="context">The Health Check Context.</param>
	/// <param name="response">The endpoint response.</param>
	/// <param name="allowedStatusCodes">The allowed status codes for the response..</param>
	/// <returns>The health check result.</returns>
	public static async Task<HealthCheckResult> CheckResponseStatusCodeAsync(
		HealthCheckContext context,
		HttpResponseMessage response,
		HttpStatusCode[]? allowedStatusCodes = null)
	{
		if (allowedStatusCodes == null || allowedStatusCodes.Length == 0)
		{
			allowedStatusCodes = new[] { HttpStatusCode.OK };
		}

		if (allowedStatusCodes.Contains(response.StatusCode))
		{
			return await Task.FromResult(new HealthCheckResult(HealthStatus.Healthy, "The endpoint returned an allowed status code."));
		}

		return await Task.FromResult(
			new HealthCheckResult(
				context.Registration.FailureStatus,
				$"The endpoint returned an unallowed status code. Status code returned: '{response.StatusCode}'. " +
				$"Allowed status codes: '{string.Join(", ", allowedStatusCodes.Select(h => h.ToString()))}'"));
	}
}
