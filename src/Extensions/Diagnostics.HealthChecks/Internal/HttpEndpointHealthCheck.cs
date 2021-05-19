// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class HttpEndpointHealthCheck : AbstractHealthCheck<HttpHealthCheckParameters>
	{
		private const string Host = "localhost";

		public static string HttpClientLogicalName { get; } = "HttpEndpointHealthCheckHttpClient";

		private readonly IHttpClientFactory m_httpClientFactory;

		private Uri? m_uriToCheck;

		public HttpEndpointHealthCheck(
			HttpHealthCheckParameters parameters,
			IHttpClientFactory httpClientFactory,
			ILogger<HttpEndpointHealthCheck> logger,
			ActivitySource activitySource)
				: base(parameters, logger, activitySource)
		{
			m_httpClientFactory = httpClientFactory;
		}

		protected override async Task<HealthCheckResult> CheckHealthInternalAsync(HealthCheckContext context, CancellationToken token)
		{
			string checkName = context.Registration.Name;

			if (m_uriToCheck == null)
			{
				int port = SfConfigurationProvider.GetEndpointPort(Parameters.EndpointName);
				UriBuilder builder = new UriBuilder(Parameters.Scheme, Host, port, Parameters.RelativeUri.ToString());
				m_uriToCheck = builder.Uri;
			}

			HttpClient httpClient = m_httpClientFactory.CreateClient(HttpClientLogicalName);

			HttpRequestMessage request = new HttpRequestMessage(Parameters.Method, m_uriToCheck);

			foreach (KeyValuePair<string, IEnumerable<string>> pair in Parameters.Headers)
			{
				if (!request.Headers.TryAddWithoutValidation(pair.Key, pair.Value))
				{
					string logMessage = string.Format(
						CultureInfo.InvariantCulture,
						"Cannot add request header with name '{0}' value '{1}' for health check '{2}'.",
						pair.Key,
						pair.Value,
						checkName);
					Logger.LogWarning(Tag.Create(), logMessage);

					return new HealthCheckResult(HealthStatus.Unhealthy, logMessage, data: Parameters.ReportData);
				}
			}

			HttpResponseMessage? response = await httpClient.SendAsync(request, token).ConfigureAwait(false);

			HealthStatus healthStatus = HealthStatus.Unhealthy;
			string description = string.Empty;

			if (response != null)
			{
				if (response.StatusCode == Parameters.ExpectedStatus)
				{
					healthStatus = HealthStatus.Healthy;
				}
				else
				{
					Logger.LogWarning(Tag.Create(), "'{0}' is unhealthy, expected request result {1}, actual {2}",
						checkName, Parameters.ExpectedStatus, response.StatusCode);

					// attach response to description only if check is not healthy to improve performance
					description = await response.Content.ReadAsStringAsync();
				}
			}

			HealthCheckResult result = new HealthCheckResult(healthStatus, description, data: Parameters.ReportData);

			if (Parameters.AdditionalCheck != null && response != null)
			{
				Logger.LogInformation(Tag.Create(), "'{0}' check result will be overridden by {1}",
					checkName, nameof(HttpHealthCheckParameters.AdditionalCheck));

				result = Parameters.AdditionalCheck(response, result);
			}

			return result;
		}
	}
}
