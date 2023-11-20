// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// A collection of extension methods to define <seealso cref="HttpClient"/> for Health Checks
/// in the DI.
/// </summary>
public static class HealthCheckHttpClientServiceCollectionExtensions
{
	/// <summary>
	/// Defines a <seealso cref="HttpClient"/> in the DI designed for Health Checks.
	/// The main difference is that these clients in development will ignore SSL errors.
	/// This is an acceptable compromise considering that the health checks are executed in the same machine
	/// as the Service Fabric service itself, and that each client will be specifically used for the
	/// health checks, as the name parameter in input is mandatory.
	/// Every other uses of this extension methods apart from health checks should be avoided.
	/// </summary>
	/// <param name="services">The services.</param>
	/// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
	/// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
	public static IHttpClientBuilder AddHealthCheckHttpClient(this IServiceCollection services, string name) =>
		services.AddHttpClient(name)
			.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (_, _, _, _) => true
			});
}
