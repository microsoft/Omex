// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Omex.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering <see cref="HealthCheckService"/> in an <see cref="IServiceCollection"/>.
/// </summary>
public static class OmexHealthCheckServiceCollectionExtensions
{
	/// <summary>
	/// Adds the <see cref="HealthCheckService"/> to the container, using the provided delegate to register
	/// health checks.
	/// </summary>
	/// <remarks>
	/// This operation is idempotent - multiple invocations will still only result in a single
	/// <see cref="HealthCheckService"/> instance in the <see cref="IServiceCollection"/>. It can be invoked
	/// multiple times in order to get access to the <see cref="IOmexHealthChecksBuilder"/> in multiple places.
	/// </remarks>
	/// <param name="services">The <see cref="IServiceCollection"/> to add the <see cref="HealthCheckService"/> to.</param>
	/// <returns>An instance of <see cref="IOmexHealthChecksBuilder"/> from which health checks can be registered.</returns>
	public static IOmexHealthChecksBuilder AddOmexHealthChecks(this IServiceCollection services)
	{
		services.AddHealthChecks(); // Register DefaultHealthCheckService

		// Register OmexHealthCheckPublisherHostedService
		services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, OmexHealthCheckPublisherHostedService>());
		return new OmexHealthChecksBuilder(services);
	}
}
