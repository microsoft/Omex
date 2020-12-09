// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	/// <summary>
	/// Extension to add health checks into IHealthChecksBuilder
	/// </summary>
	public static class HealthChecksBuilderExtensions
	{
		/// <summary>
		/// Add http endpoint health check
		/// </summary>
		/// <param name="builder">health checks builder</param>
		/// <param name="name">name of the health check</param>
		/// <param name="endpointName">name of the endpoint to check</param>
		/// <param name="relativePath">relative path to check, absolute path not allowed</param>
		/// <param name="method">http method to use, defaults to HttpGet</param>
		/// <param name="scheme">uri scheme, defaults to http</param>
		/// <param name="headers">headers to attach to the request</param>
		/// <param name="expectedStatus">response status code that considered healthy, default to 200(OK)</param>
		/// <param name="additionalCheck">action that would be called after getting response, function should return new result object that would be reported</param>
		/// <param name="reportData">additional properties that will be attached to health check result, for example escalation info</param>
		public static IHealthChecksBuilder AddHttpEndpointCheck(
			this IHealthChecksBuilder builder,
			string name,
			string endpointName,
			string relativePath,
			HttpMethod? method = null,
			string? scheme = null,
			IDictionary<string, string>? headers = null,
			HttpStatusCode? expectedStatus = null,
			Func<HttpResponseMessage, HealthCheckResult, HealthCheckResult>? additionalCheck = null,
			params KeyValuePair<string, object>[] reportData)
		{
			return builder.AddTypeActivatedCheck<HttpEndpointHealthCheck>(
				name,
				new HttpHealthCheckParameters(endpointName, new Uri(relativePath, UriKind.Relative), method, scheme, headers, expectedStatus, additionalCheck, reportData));
		}
	}
}
