﻿// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
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

	/// <summary>
	/// Adds the endpoint helath check in the project.
	/// This overload will build the custom Health Check parameters with the parameters provided in input.
	/// The order of the input parameters is the same as the legacy extension method.
	/// </summary>
	/// <param name="healthChecksBuilder">The HealthChecks builder.</param>
	/// <param name="name">The Health Check name.</param>
	/// <param name="endpointName">The Service Fabric endpoint name.</param>
	/// <param name="relativePath">The relative path of the endpoint to check.</param>
	/// <param name="httpClientLogicalName">
	/// The HTTP Client logical name. It must be the same as the one registered in the DI.
	/// </param>
	/// <param name="failureStatus">The status that will be reported if the Health Check fails.</param>
	/// <param name="host">The service host.</param>
	/// <param name="reportData">The report data parameters.</param>
	/// <returns>The Health Check builder.</returns>
	public static IHealthChecksBuilder AddEndpointHttpHealthCheck(
		this IHealthChecksBuilder healthChecksBuilder,
		string name,
		string endpointName,
		string relativePath,
		string httpClientLogicalName,
		HealthStatus failureStatus = HealthStatus.Unhealthy,
		string host = "localhost",
		params KeyValuePair<string, object>[] reportData)
	{
		EndpointLivenessHealthCheckParameters parameters = new(
			endpointName,
			httpClientLogicalName,
			relativePath,
			host,
			reportData
		);

		return healthChecksBuilder.AddEndpointHttpHealthCheck(name, failureStatus, parameters);
	}
}
