// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class HttpEndpointHealthCheck : AbstractHealthCheck
	{
		private const string Host = "localhost";

		public static string HttpClientLogicalName { get; } = "HttpEndpointHealthCheckHttpClient";

		internal HttpHealthCheckParameters Parameters { get; } // internal only to be used for unit tests

		private readonly IHttpClientFactory m_httpClientFactory;

		private readonly IAccessor<ServiceContext> m_accessor;

		private Uri? m_uriToCheck;

		public HttpEndpointHealthCheck(
			HttpHealthCheckParameters parameters,
			IHttpClientFactory httpClientFactory,
			IAccessor<ServiceContext> accessor,
			ITimedScopeProvider scopeProvider)
				: base(scopeProvider)
		{
			Parameters = parameters;
			m_httpClientFactory = httpClientFactory;
			m_accessor = accessor;
		}

		protected override async Task<HealthCheckResult> CheckHealthInternalAsync(HealthCheckContext context, CancellationToken token)
		{
			if (m_accessor.Value == null)
			{
				return new HealthCheckResult(HealthStatus.Degraded, "Not initialized");
			}

			if (m_uriToCheck == null)
			{
				int port = m_accessor.Value.CodePackageActivationContext.GetEndpoint(Parameters.EndpointName).Port;
				UriBuilder builder = new UriBuilder(Parameters.Scheme, Host, port, Parameters.RelativeUri.ToString());
				m_uriToCheck = builder.Uri;
			}

			HttpClient httpClient = m_httpClientFactory.CreateClient(HttpClientLogicalName);

			HttpRequestMessage request = new HttpRequestMessage(Parameters.Method, m_uriToCheck);

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
					// attach response to description only if check is not healthy to improve performance
					description = await response.Content.ReadAsStringAsync();
				}
			}

			HealthCheckResult result = new HealthCheckResult(healthStatus, description, data: Parameters.ReportData);

			if (Parameters.AdditionalCheck != null && response != null)
			{
				result = Parameters.AdditionalCheck(response, result);
			}

			return result;
		}
	}
}
