// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class HttpEndpointHealthCheck : IHealthCheck
	{
		public static string HttpClientLogicalName { get; } = "HttpEndpointHealthCheckHttpClient";

		private const string Host = "localhost";

		private readonly IHttpClientFactory m_httpClientFactory;

		private readonly HttpHealthCheckParameters m_parameters;

		private readonly IAccessor<ServiceContext> m_accessor;

		private Uri? m_uriToCheck;

		public HttpEndpointHealthCheck(IHttpClientFactory httpClientFactory, HttpHealthCheckParameters parameters, IAccessor<ServiceContext> accessor)
		{
			m_httpClientFactory = httpClientFactory;
			m_parameters = parameters;
			m_accessor = accessor;
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = default)
		{
			if (m_accessor.Value == null)
			{
				return new HealthCheckResult(HealthStatus.Degraded, "Not initialized");
			}

			try
			{
				if (m_uriToCheck == null)
				{
					int port = m_accessor.Value.CodePackageActivationContext.GetEndpoint(m_parameters.EndpointName).Port;
					UriBuilder builder = new UriBuilder(m_parameters.Scheme, Host, port, m_parameters.RelativeUri.ToString());
					m_uriToCheck = builder.Uri;
				}

				HttpClient httpClient = m_httpClientFactory.CreateClient(HttpClientLogicalName);

				HttpRequestMessage request = new HttpRequestMessage(m_parameters.Method, m_uriToCheck);

				HttpResponseMessage? response = await httpClient.SendAsync(request, token).ConfigureAwait(false);

				HealthStatus healthStatus = response?.StatusCode == HttpStatusCode.OK
					? HealthStatus.Healthy
					: HealthStatus.Unhealthy;

				HealthCheckResult result = new HealthCheckResult(healthStatus, data: m_parameters.ReportData);

				if (m_parameters.AdditionalCheck != null && response != null)
				{
					m_parameters.AdditionalCheck(response, result);
				}

				return result;
			}
			catch (Exception exception)
			{
				return HealthCheckResult.Unhealthy("Request failed", exception);
			}
		}
	}
}
