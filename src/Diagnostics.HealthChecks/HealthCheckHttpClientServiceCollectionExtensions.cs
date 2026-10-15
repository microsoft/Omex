// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Net.Http;
using System.Net.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// A collection of extension methods to define <seealso cref="HttpClient"/> for Health Checks
/// in the DI.
/// </summary>
public static class HealthCheckHttpClientServiceCollectionExtensions
{
	/// <summary>
	/// Defines a <seealso cref="HttpClient"/> in the DI designed for Health Checks.
	/// <para>
	/// Server certificate errors are ignored only for requests that target the local machine, meaning a
	/// loopback host such as <c>localhost</c>, <c>127.0.0.1</c> or <c>[::1]</c>, or a bare IP literal.
	/// This is an acceptable compromise because health checks are executed on the same machine as the
	/// Service Fabric service itself and are commonly addressed by node IP rather than by the subject
	/// name present on the certificate, so validation cannot succeed by construction.
	/// </para>
	/// <para>
	/// Requests that target a DNS name, meaning any external or FQDN addressed endpoint, are fully
	/// validated. Use <see cref="AddValidatedHealthCheckHttpClient"/> when the health check only ever
	/// calls remote endpoints and the local relaxation is not wanted at all.
	/// </para>
	/// <para>
	/// In the Development environment all certificate errors are ignored, including for DNS hosts, to
	/// support self signed certificates on developer machines.
	/// </para>
	/// <para>
	/// Every other use of this extension method apart from health checks should be avoided.
	/// </para>
	/// </summary>
	/// <param name="services">The services.</param>
	/// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
	/// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
	public static IHttpClientBuilder AddHealthCheckHttpClient(this IServiceCollection services, string name) =>
		services.AddHealthCheckHttpClient(name, allowLocalCertificateErrors: true);

	/// <summary>
	/// Defines a <seealso cref="HttpClient"/> in the DI designed for Health Checks that always validates
	/// server certificates, including for loopback and IP addressed endpoints.
	/// <para>
	/// The only exception is the Development environment, where certificate errors are ignored to support
	/// self signed certificates on developer machines.
	/// </para>
	/// <para>
	/// Prefer this method when the health check targets an endpoint that presents a certificate valid for
	/// the address being called.
	/// </para>
	/// </summary>
	/// <param name="services">The services.</param>
	/// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
	/// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
	public static IHttpClientBuilder AddValidatedHealthCheckHttpClient(this IServiceCollection services, string name) =>
		services.AddHealthCheckHttpClient(name, allowLocalCertificateErrors: false);

	private static IHttpClientBuilder AddHealthCheckHttpClient(this IServiceCollection services, string name, bool allowLocalCertificateErrors) =>
		services.AddHttpClient(name)
			.ConfigurePrimaryHttpMessageHandler(provider =>
			{
				// If the environment cannot be resolved we keep the secure default of treating the
				// application as non Development.
				bool isDevelopment = provider.GetService<IHostEnvironment>()?.IsDevelopment() == true;

				return new HttpClientHandler
				{
					ServerCertificateCustomValidationCallback = (request, _, _, errors) =>
						errors == SslPolicyErrors.None
						|| isDevelopment
						|| (allowLocalCertificateErrors && IsLocalEndpoint(request.RequestUri))
				};
			});

	/// <summary>
	/// Determines whether a request targets the local machine, either through a loopback host or through
	/// a bare IP literal, in which case a certificate issued for a DNS name cannot match the address used.
	/// </summary>
	private static bool IsLocalEndpoint(Uri? requestUri) =>
		requestUri != null
		&& (requestUri.IsLoopback
			|| requestUri.HostNameType == UriHostNameType.IPv4
			|| requestUri.HostNameType == UriHostNameType.IPv6);
}
