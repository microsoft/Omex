// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
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
	/// loopback host such as <c>localhost</c>, <c>127.0.0.1</c> or <c>[::1]</c>, or an IP address assigned
	/// to a network interface on this machine.
	/// This is an acceptable compromise because health checks are executed on the same machine as the
	/// Service Fabric service itself and are commonly addressed by node IP rather than by the subject
	/// name present on the certificate, so validation cannot succeed by construction.
	/// </para>
	/// <para>
	/// Requests that target a non-loopback DNS name or an IP address on another machine are fully
	/// validated. Use <see cref="AddValidatedHealthCheckHttpClient"/> when the health check only ever
	/// calls remote endpoints and the local relaxation is not wanted at all.
	/// </para>
	/// <para>
	/// In the Development environment all certificate errors are ignored, including for DNS hosts, to
	/// support self signed certificates on developer machines.
	/// </para>
	/// <para>
	/// Every other use of this extension method apart from health checks should be avoided.
	/// Automatic Windows authentication is not enabled; consumers must explicitly configure credentials
	/// if their health check requires authentication.
	/// </para>
	/// </summary>
	/// <param name="services">The services.</param>
	/// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
	/// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
	public static IHttpClientBuilder AddHealthCheckHttpClient(this IServiceCollection services, string name) =>
		services.AddHealthCheckHttpClient(name, allowLocalCertificateErrors: true, allowAutoRedirect: true);

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
		services.AddHealthCheckHttpClient(name, allowLocalCertificateErrors: false, allowAutoRedirect: true);

	internal static IHttpClientBuilder AddHealthCheckHttpClient(this IServiceCollection services, string name, bool allowLocalCertificateErrors, bool allowAutoRedirect) =>
		services.AddHttpClient(name)
			.ConfigurePrimaryHttpMessageHandler(provider =>
			{
				// If the environment cannot be resolved we keep the secure default of treating the
				// application as non Development.
				bool isDevelopment = provider.GetService<IHostEnvironment>()?.IsDevelopment() == true;

				return new HttpClientHandler
				{
					AllowAutoRedirect = allowAutoRedirect,
					ServerCertificateCustomValidationCallback = (request, _, _, errors) =>
						errors == SslPolicyErrors.None
						|| isDevelopment
						|| (allowLocalCertificateErrors && IsLocalEndpoint(request.RequestUri))
				};
			});

	/// <summary>
	/// Determines whether a request targets the local machine, either through a loopback host or through
	/// an IP address assigned to one of its network interfaces.
	/// </summary>
	private static bool IsLocalEndpoint(Uri? requestUri) =>
		requestUri != null
		&& (requestUri.IsLoopback
			|| (IPAddress.TryParse(requestUri.DnsSafeHost, out IPAddress? address)
				&& NetworkInterface.GetAllNetworkInterfaces()
					.SelectMany(networkInterface => networkInterface.GetIPProperties().UnicastAddresses)
					.Any(unicastAddress => unicastAddress.Address.Equals(address))));
}
