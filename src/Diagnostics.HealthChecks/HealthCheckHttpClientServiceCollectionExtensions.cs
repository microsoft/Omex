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
	/// Uses platform certificate validation in all environments. HTTPS endpoints must present
	/// trusted certificates that match the configured host.
	/// </summary>
	/// <param name="services">The services.</param>
	/// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
	/// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
	public static IHttpClientBuilder AddHealthCheckHttpClient(this IServiceCollection services, string name) =>
		services.AddHttpClient(name)
			.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler());
}
