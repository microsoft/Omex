// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Omex.Extensions.DependencyInjection;
using Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	/// <summary>
	/// Extension to add Omex dependencies to IServiceCollection
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Add dependencies for a publisher
		/// </summary>
		[Obsolete("This method is deprecated and will be removed in a later release, please use HealthCheckComposablesExtensions class extension methods to compose health checks.")]
		private static IServiceCollection AddPublisherDependencies(this IServiceCollection serviceCollection)
		{
			// HttpClient registration only needed for HttpEndpointHealthCheck.
			// It add added here instead of AddHttpEndpointCheck method to avoid creating new configuration each time new health check added.
			// It would be nice to register this HttpClient configuration once and only if HttpEndpointCheck used.
			serviceCollection
				.AddHttpClient(HttpEndpointHealthCheck.HttpClientLogicalName)
				.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
				{
					AllowAutoRedirect = false,
					Credentials = CredentialCache.DefaultCredentials,
					ServerCertificateCustomValidationCallback = (sender, x509Certificate, chain, errors) => true
				});

			serviceCollection.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
			return serviceCollection;
		}

		/// <summary>
		/// Register publisher for processing health check results directly to replicas
		/// </summary>
		[Obsolete("This method is deprecated and will be removed in a later release, please use HealthCheckComposablesExtensions class extension methods to compose health checks.")]
		public static IHealthChecksBuilder AddServiceFabricHealthChecks(this IServiceCollection serviceCollection) =>
			serviceCollection.AddOmexHealthCheckDependencies<ServiceContextHealthStatusSender>();

		/// <summary>
		/// Register publisher for processing health check results directly to nodes using REST api
		/// </summary>
		[Obsolete("This method is deprecated and will be removed in a later release, please use HealthCheckComposablesExtensions class extension methods to compose health checks.")]
		public static IHealthChecksBuilder AddRestHealthChecksPublisher(this IServiceCollection serviceCollection) =>
			serviceCollection
				.AddServiceFabricClient()
				.AddOmexHealthCheckDependencies<RestHealthStatusSender>();

		/// <summary>
		/// Register publisher for processing health check results
		/// </summary>

		[Obsolete("This method is deprecated and will be removed in a later release, please use HealthCheckComposablesExtensions class extension methods to compose health checks.")]
		private static IHealthChecksBuilder AddOmexHealthCheckDependencies<TStatusSender>(this IServiceCollection serviceCollection)
				where TStatusSender : class, IHealthStatusSender
		{
			serviceCollection.TryAddSingleton<IHealthStatusSender, TStatusSender>();
			return serviceCollection
				.AddPublisherDependencies()
				.AddHealthCheckPublisher<OmexHealthCheckPublisher>()
				.AddOmexHealthChecks();
		}

		/// <summary>
		/// Register publisher using Dependency Injection
		/// </summary>
		public static IServiceCollection AddHealthCheckPublisher<TPublisher>(this IServiceCollection serviceCollection)
			where TPublisher : class, IHealthCheckPublisher
		{
			serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheckPublisher, TPublisher>());
			return serviceCollection;
		}
	}
}
