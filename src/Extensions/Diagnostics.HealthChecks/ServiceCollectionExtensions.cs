// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Fabric;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	/// <summary>
	/// Extension to add Omex dependencies to IServiceCollection
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Register types required for Omex health check
		/// </summary>
		public static IHealthChecksBuilder AddServiceFabricHealthChecks(this IServiceCollection serviceCollection)
		{
			HttpClientHandler clientHandler = new HttpClientHandler
			{
				AllowAutoRedirect = false,
				Credentials = CredentialCache.DefaultCredentials,
				ServerCertificateCustomValidationCallback = (sender, x509Certificate, chain, errors) => true
			};

			serviceCollection
				.AddHttpClient(HttpEndpointHealthCheck.HttpClientLogicalName)
				.ConfigurePrimaryHttpMessageHandler(() => clientHandler);

			return serviceCollection
				.AddHealthCheckPublisher<ServiceFabricHealthCheckPublisher>()
				.AddHealthCheckPublisher<EscalationHealthCheckPublisher>()
				.AddHealthChecks();
		}

		/// <summary>
		/// Register publisher for processing health check results
		/// </summary>
		public static IServiceCollection AddHealthCheckPublisher<TPublisher>(this IServiceCollection serviceCollection)
			where TPublisher : class, IHealthCheckPublisher
		{
			serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheckPublisher, TPublisher>());
			return serviceCollection;
		}
	}

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
		/// <param name="relatedUri">relative uri with path to check</param>
		/// <param name="method">http method to use, defaults to HttpGet</param>
		/// <param name="scheme">uri scheme, defaults to http</param>
		/// <param name="additionalCheck">action that would be called after getting response, it allows modifying health check result</param>
		/// <param name="reportData">additional properties that will be attached to health check result, for example escalation info</param>
		public static IHealthChecksBuilder AddHttpEndpointCheck(
			this IHealthChecksBuilder builder,
			string name,
			string endpointName,
			Uri relatedUri,
			HttpMethod? method = null,
			string? scheme = null,
			Action<HttpResponseMessage, HealthCheckResult>? additionalCheck = null,
			params KeyValuePair<string, object>[] reportData)
		{
			return builder.AddTypeActivatedCheck<HttpEndpointHealthCheck>(
				name,
				new HttpHealthCheckParameters(endpointName, relatedUri, method, scheme, additionalCheck, reportData));
		}
	}

	internal class HttpEndpointHealthCheck : IHealthCheck
	{
		public static string HttpClientLogicalName { get; } = "HttpEndpointHealthCheckHttpClient";

		private const string Host = "localhost";

		private readonly IHttpClientFactory m_httpClientFactory;

		private readonly HttpHealthCheckParameters m_parameters;

		private Uri? m_uriToCheck;

		public HttpEndpointHealthCheck(IHttpClientFactory httpClientFactory, HttpHealthCheckParameters parameters, IAccessor<ServiceContext> accessor)
		{
			m_httpClientFactory = httpClientFactory;
			m_parameters = parameters;
			accessor.OnUpdated(SetUri);
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = default)
		{
			if (m_uriToCheck == null)
			{
				return new HealthCheckResult(HealthStatus.Degraded, "Not initialized");
			}

			try
			{
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

		private void SetUri(ServiceContext context)
		{
			int port = context.CodePackageActivationContext.GetEndpoint(m_parameters.EndpointName).Port;
			UriBuilder builder = new UriBuilder(m_parameters.Scheme, Host, port, m_parameters.RelativeUri.ToString());
			m_uriToCheck = builder.Uri;
		}
	}

	internal class HttpHealthCheckParameters
	{
		public string EndpointName { get; }
		public Uri RelativeUri { get; }
		public HttpMethod Method { get; }
		public string Scheme { get; }
		public Action<HttpResponseMessage, HealthCheckResult>? AdditionalCheck { get; }
		public IReadOnlyDictionary<string, object> ReportData { get; }

		public HttpHealthCheckParameters(
			string endpointName,
			Uri relatedUri,
			HttpMethod? method,
			string? scheme,
			Action<HttpResponseMessage, HealthCheckResult>? additionalCheck,
			IEnumerable<KeyValuePair<string, object>> reportData)
		{
			EndpointName = string.IsNullOrWhiteSpace(endpointName)
				? throw new ArgumentException("Invalid endpoint name", nameof(endpointName))
				: endpointName;

			RelativeUri = relatedUri.IsAbsoluteUri
				? throw new ArgumentException("Absolute uri not allowed", nameof(relatedUri))
				: relatedUri;

			Method = method ?? HttpMethod.Get;

			Scheme = scheme == null
				? Uri.UriSchemeHttp
				: Uri.CheckSchemeName(scheme)
					? scheme
					: throw new ArgumentException("Invalid uri scheme", nameof(scheme));

			AdditionalCheck = additionalCheck;

			ReportData = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(reportData));
		}
	}
}
