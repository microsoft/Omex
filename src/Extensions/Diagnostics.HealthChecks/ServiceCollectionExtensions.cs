// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Mtyrolski
{
	/// <summary>
	/// Extension to add Omex dependencies to IServiceCollection
	/// </summary>
	public static class ServiceCollectionExtensions
	{
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
		public static IHealthChecksBuilder AddServiceFabricHealthChecks<TPublisher>(this IServiceCollection serviceCollection)
				where TPublisher : class, IHealthCheckPublisher
		{
			serviceCollection.ConfigureService();
			Type publisherType = typeof(TPublisher);
			if(publisherType == typeof(RestHealthCheckPublisher))
			{
				return serviceCollection.AddRestHealthCheckPublisher().AddHealthChecks();
				
			}

			return serviceCollection.AddHealthCheckPublisher<TPublisher>().AddHealthChecks();
		}

		/// <summary>
		/// Register publisher for processing health check results
		/// </summary>
		/// <summary>
		/// Register publisher for processing health check results
		/// </summary>
		public static IServiceCollection AddHealthCheckPublisher<TPublisher>(this IServiceCollection serviceCollection)
			where TPublisher : class, IHealthCheckPublisher
		{
			serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheckPublisher, TPublisher>());
			return serviceCollection;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="serviceCollection"></param>
		/// <returns></returns>
		public static IServiceCollection AddRestHealthCheckPublisher(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheckPublisher, RestHealthCheckPublisher>(
				(serviceProvider) =>
				{
					return new RestHealthCheckPublisher(
						clusterEndpoints: new Uri(serviceProvider
													.GetRequiredService<IOptions<RestHealthCheckPublisherOptions>>()
													.Value
													.RestHealthPublisherClusterEndpoint),
						serviceId: serviceProvider
													.GetRequiredService<IOptions<RestHealthCheckPublisherOptions>>()
													.Value
													.RestHealthPublisherServiceId
					);
				}
				));
			return serviceCollection;
		}
	}
}
