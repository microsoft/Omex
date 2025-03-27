// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Abstractions;

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
		/// <param name="failureStatus">status that should be reported when the health check reports a failure, if the provided value is null, Unhealthy will be reported Unhealthy</param>
		/// <param name="additionalCheck">action that would be called after getting response, function should return new result object that would be reported</param>
		/// <param name="reportData">additional properties that will be attached to health check result, for example escalation info</param>
		[Obsolete("This method is deprecated and will be removed in a later release, please use HealthCheckComposablesExtensions.AddEndpointHttpHealthCheck in the Microsoft.Omex.Extensions.Diagnostics.HealthChecks.AspNetCore package instead.")]
		public static IHealthChecksBuilder AddHttpEndpointCheck(
			this IHealthChecksBuilder builder,
			string name,
			string endpointName,
			string relativePath,
			HttpMethod? method = null,
			string? scheme = null,
			IReadOnlyDictionary<string, IEnumerable<string>>? headers = null,
			HttpStatusCode? expectedStatus = null,
			HealthStatus failureStatus = HealthStatus.Unhealthy,
			Func<HttpResponseMessage, HealthCheckResult, Task<HealthCheckResult>>? additionalCheck = null,
			params KeyValuePair<string, object>[] reportData)
		{
			scheme = scheme == null
					? Uri.UriSchemeHttp
					: Uri.CheckSchemeName(scheme)
						? scheme
						: throw new ArgumentException("Invalid uri scheme", nameof(scheme));

			if (!Uri.TryCreate(relativePath, UriKind.Relative, out Uri? result) || result.IsAbsoluteUri)
			{
				throw new ArgumentException("relativePath is not valid or can't be an Absolute uri", nameof(relativePath));
			}

			Func<UriBuilder, HttpRequestMessage> httpRequest = uriBuilder =>
			{
				uriBuilder.Path = relativePath;
				uriBuilder.Scheme = scheme;
				HttpRequestMessage requestMessage = new(method ?? HttpMethod.Get, uriBuilder.Uri);

				if (headers != null)
				{
					foreach (KeyValuePair<string, IEnumerable<string>> pair in headers)
					{
						if (!requestMessage.Headers.TryAddWithoutValidation(pair.Key, pair.Value))
						{
							string errorMessage = string.Format(
								CultureInfo.InvariantCulture,
								"Cannot add request header with name '{0}' value '{1}' for health check '{2}'.",
								pair.Key,
								pair.Value,
								name);

							throw new ArgumentException(errorMessage, nameof(headers));
						}
					}
				}

				return requestMessage;
			};

			return builder.AddHttpEndpointCheck(name, endpointName, httpRequest, expectedStatus, failureStatus, additionalCheck, reportData);
		}

		/// <summary>
		/// Add http endpoint health check
		/// </summary>
		/// <param name="builder">health checks builder</param>
		/// <param name="name">name of the health check</param>
		/// <param name="endpointName">name of the endpoint to check</param>
		/// <param name="httpRequestMessageBuilder">action that will return the http request message</param>
		/// <param name="expectedStatus">response status code that considered healthy, default to 200(OK)</param>
		/// <param name="failureStatus">status that should be reported when the health check reports a failure, if the provided value is null, Unhealthy will be reported Unhealthy</param>
		/// <param name="additionalCheck">action that would be called after getting response, function should return new result object that would be reported</param>
		/// <param name="reportData">additional properties that will be attached to health check result, for example escalation info</param>
		[Obsolete("This method is deprecated and will be removed in a later release, please use HealthCheckComposablesExtensions.AddEndpointHttpHealthCheck in the Microsoft.Omex.Extensions.Diagnostics.HealthChecks.AspNetCore package instead.")]
		public static IHealthChecksBuilder AddHttpEndpointCheck(
			this IHealthChecksBuilder builder,
			string name,
			string endpointName,
			Func<UriBuilder, HttpRequestMessage> httpRequestMessageBuilder,
			HttpStatusCode? expectedStatus = null,
			HealthStatus? failureStatus = null,
			Func<HttpResponseMessage, HealthCheckResult, Task<HealthCheckResult>>? additionalCheck = null,
			params KeyValuePair<string, object>[] reportData)
		{
			int port = SfConfigurationProvider.GetEndpointPort(endpointName);
			UriBuilder uriBuilder = new(Uri.UriSchemeHttp, "localhost", port);
			HttpRequestMessage requestMessage = httpRequestMessageBuilder(uriBuilder);

			return builder.AddTypeActivatedCheck<HttpEndpointHealthCheck>(
				name,
				failureStatus,
				new HttpHealthCheckParameters(requestMessage, expectedStatus, additionalCheck, reportData));
		}
	}
}
