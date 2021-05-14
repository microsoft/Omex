// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.ObjectPool;
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
		private static void AddPublisherDependencies(this IServiceCollection serviceCollection)
		{
			serviceCollection
				.AddHttpClient(HttpEndpointHealthCheck.HttpClientLogicalName)
				.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
				{
					AllowAutoRedirect = false,
					Credentials = CredentialCache.DefaultCredentials,
					ServerCertificateCustomValidationCallback = (sender, x509Certificate, chain, errors) => true
				});

			serviceCollection.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
		}

		/// <summary>
		/// Register publisher for processing health check results directly to replicas
		/// </summary>
		public static IHealthChecksBuilder AddServiceFabricHealthChecks(this IServiceCollection serviceCollection)
		{
			return serviceCollection.AddServiceFabricHealthChecks<ServiceFabricHealthCheckPublisher>();
		}

		/// <summary>
		/// Register publisher for processing health check results directly to nodes using REST api
		/// </summary>
		public static IHealthChecksBuilder AddRestHealthChecks(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddServiceFabricClient();
			return serviceCollection.AddServiceFabricHealthChecks<RestHealthCheckPublisher>();
		}

		/// <summary>
		/// Register publisher for processing health check results
		/// </summary>
		public static IHealthChecksBuilder AddServiceFabricHealthChecks<TPublisher>(this IServiceCollection serviceCollection)
				where TPublisher : class, IHealthCheckPublisher
		{
			serviceCollection.AddPublisherDependencies();
			return serviceCollection.AddHealthCheckPublisher<TPublisher>().AddHealthChecks();
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
