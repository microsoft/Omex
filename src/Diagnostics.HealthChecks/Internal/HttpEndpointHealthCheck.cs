// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	[Obsolete("The usage of this class is deprecated and will be removed in a later release, please use composable classes in Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Composables namespace to build health checks.")]
	internal class HttpEndpointHealthCheck : AbstractHealthCheck<HttpHealthCheckParameters>
	{
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

			HttpClient httpClient = m_httpClientFactory.CreateClient(HealthCheckConstants.HttpClientLogicalName);
			HttpResponseMessage? response = await httpClient.SendAsync(CloneRequestMessage(Parameters.RequestMessage), token).ConfigureAwait(false);

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

			HealthCheckResult result = new(healthStatus, description, data: Parameters.ReportData);

			if (Parameters.AdditionalCheck != null && response != null)
			{
				Logger.LogInformation(Tag.Create(), "'{0}' check result will be overridden by {1}",
					checkName, nameof(HttpHealthCheckParameters.AdditionalCheck));

				result = await Parameters.AdditionalCheck(response, result).ConfigureAwait(false);
			}

			return result;
		}

		private static HttpRequestMessage CloneRequestMessage(HttpRequestMessage message)
		{
			HttpRequestMessage clone = new()
			{
				Method = message.Method,
				RequestUri = message.RequestUri,
				Content = message.Content,
				Version = message.Version
			};

			foreach (KeyValuePair<string, IEnumerable<string>> header in message.Headers)
			{
				clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

#if !NETCOREAPP3_1 && !NETSTANDARD2_0
			clone.VersionPolicy = message.VersionPolicy;

			foreach (KeyValuePair<string, object?> option in message.Options)
			{
				clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
			}
#else
			foreach (KeyValuePair<string, object> prop in message.Properties)
			{
				clone.Properties.Add(prop.Key, prop.Value);
			}
#endif

			return clone;
		}
	}
}
