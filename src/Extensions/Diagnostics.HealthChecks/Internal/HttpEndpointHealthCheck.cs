// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class HttpEndpointHealthCheck : AbstractHealthCheck<HttpHealthCheckParameters>
	{
		public static string HttpClientLogicalName { get; } = "HttpEndpointHealthCheckHttpClient";

		private readonly IHttpClientFactory m_httpClientFactory;

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

			HttpClient httpClient = m_httpClientFactory.CreateClient(HttpClientLogicalName);

			HttpRequestMessage request = Parameters.HttpRequest;

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
