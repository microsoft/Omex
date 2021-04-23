// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Net;
using System.Fabric;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	/// <summary>
	/// Extension to add Omex dependencies to IServiceCollection
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="serviceCollection"></param>
		private static void ConfigureService(this IServiceCollection serviceCollection)
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
		/// 
		/// </summary>
		/// <param name="serviceCollection"></param>
		/// <returns></returns>
		public static IHealthChecksBuilder AddServiceFabricHealthChecks(this IServiceCollection serviceCollection)
		{
			return serviceCollection.AddServiceFabricHealthChecks<ServiceFabricHealthCheckPublisher>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="serviceCollection"></param>
		/// <returns></returns>
		public static IHealthChecksBuilder AddRestHealthChecks(this IServiceCollection serviceCollection)
		{
			//TODO: remove
			serviceCollection.TryAddSingleton(
				provider =>
				{
					return provider.GetRequiredService<IOptions<RestHealthCheckPublisherOptions>>().Value;
				});
			return serviceCollection.AddServiceFabricHealthChecks<RestHealthCheckPublisher>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="serviceCollection"></param>
		/// <returns></returns>
		public static IHealthChecksBuilder AddServiceFabricHealthChecks<TPublisher>(this IServiceCollection serviceCollection)
				where TPublisher : class, IHealthCheckPublisher
		{
			serviceCollection.ConfigureService();
			return serviceCollection.AddHealthCheckPublisher<TPublisher>().AddHealthChecks();
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
}
