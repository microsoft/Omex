// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.AspNetCore;

/// <summary>
/// Extension methods for health check decorators configuration.
/// </summary>
public static class HealthCheckComposablesExtensions
{
	/// <summary>
	/// Creates a new liveness HTTP endpoint checker health check.
	/// This health check will only verify the reachability of the endpoint, it will mark the activity as a liveness activity,
	/// that can trigger the response short-circuiting.
	/// </summary>
	/// <param name="requestBuilder">The request builder.</param>
	/// <param name="activitySource">The activity source.</param>
	/// <param name="httpClientFactory">The http client factory.</param>
	/// <param name="httpClientName">The HTTP Client name, if a client with that name was registered in particular.</param>
	/// <returns>The HTTP endpoint checker health check.</returns>
	public static IHealthCheck CreateLivenessHttpHealthCheck(
		Func<HttpRequestMessage> requestBuilder,
		ActivitySource activitySource,
		IHttpClientFactory httpClientFactory,
		string? httpClientName = "") =>
			Composables.HealthCheckComposablesExtensions.CreateHttpHealthCheck(
				requestBuilder,
				async (context, response, _) => await Composables.HealthCheckComposablesExtensions.CheckResponseStatusCodeAsync(context, response),
				activitySource,
				httpClientFactory,
				httpClientName: httpClientName,
				activityMarker: activity => activity?.MarkAsLivenessCheck());

	/// <summary>
	/// Adds the endpoint health check to the project.
	/// </summary>
	/// <param name="healthChecksBuilder">The health check builder.</param>
	/// <param name="name">The health check name.</param>
	/// <param name="failureStatus">
	/// The health check failure status that will be reported if the health check fails.
	/// </param>
	/// <param name="parameters">
	/// The health check parameters. Notice that these parameters will have to be an instance of the <see cref="EndpointLivenessHealthCheckParameters"/> class.
	/// </param>
	/// <returns>The health check builder.</returns>
	public static IHealthChecksBuilder AddEndpointHttpHealthCheck(
		this IHealthChecksBuilder healthChecksBuilder,
		string name,
		HealthStatus? failureStatus,
		EndpointLivenessHealthCheckParameters parameters) =>
		healthChecksBuilder
			.AddTypeActivatedCheck<EndpointLivenessHealthCheck>(
				name,
				failureStatus,
				parameters);
}
