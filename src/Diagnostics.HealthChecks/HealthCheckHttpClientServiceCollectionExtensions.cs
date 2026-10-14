// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net.Http;
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
	/// The main difference is that these clients will ignore SSL errors when, and only when, the application
	/// is running in the Development environment.
	/// This is an acceptable compromise in development considering that the health checks are executed in the
	/// same machine as the Service Fabric service itself, and that each client will be specifically used for the
	/// health checks, as the name parameter in input is mandatory.
	/// In every other environment the default certificate validation is preserved.
	/// Every other uses of this extension methods apart from health checks should be avoided.
	/// </summary>
	/// <param name="services">The services.</param>
	/// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
	/// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
	public static IHttpClientBuilder AddHealthCheckHttpClient(this IServiceCollection services, string name) =>
		services.AddHttpClient(name)
			.ConfigurePrimaryHttpMessageHandler(provider =>
			{
				HttpClientHandler handler = new();

				// Certificate validation is only bypassed in Development. If the environment cannot be
				// resolved we keep the secure default of validating server certificates.
				if (provider.GetService<IHostEnvironment>()?.IsDevelopment() == true)
				{
					handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
				}

				return handler;
			});
}
