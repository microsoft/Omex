// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.Omex.Extensions.Hosting.Services.Web;
using Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

namespace Microsoft.Extensions.Hosting
{
	/// <summary>
	/// Extension to add Omex dependencies to HostBuilder
	/// </summary>
	public static class HostBuilderExtensions
	{
		private const ServiceFabricIntegrationOptions DefaultServiceFabricIntegrationOptions = ServiceFabricIntegrationOptions.UseReverseProxyIntegration;

		/// <summary>
		/// Build stateless service with Kestrel service listener
		/// </summary>
		/// <param name="hostBuilder">Host builder</param>
		/// <param name="serviceName">Name of Service Fabric stateless service</param>
		/// <param name="endpoint">Web listener endpoint name</param>
		/// <param name="integrationOptions">Service Fabric integration options, default value is UseReverseProxyIntegration</param>
		/// <param name="kestrelOptions">Action to configure Kestrel additional, for example witch to https</param>
		/// <param name="serviceBuilder">Action to configure SF stateless service additional, for example to add more listeners or actions</param>
		/// <typeparam name="TStartup">The type containing the startup methods for the web listener</typeparam>
		public static IHost BuildStatelessWebService<TStartup>(
			this IHostBuilder hostBuilder,
			string serviceName,
			WebEndpointInfo endpoint,
			ServiceFabricIntegrationOptions integrationOptions = DefaultServiceFabricIntegrationOptions,
			Action<WebHostBuilderContext, KestrelServerOptions>? kestrelOptions = null,
			Action<ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>>? serviceBuilder = null)
				where TStartup : class =>
					hostBuilder.BuildStatelessWebService<TStartup>(
						serviceName,
						new[] { endpoint },
						integrationOptions,
						kestrelOptions,
						serviceBuilder);

		/// <summary>
		/// Build stateless service with Kestrel service listeners
		/// </summary>
		/// <param name="hostBuilder">Host builder</param>
		/// <param name="serviceName">Name of Service Fabric stateless service</param>
		/// <param name="endpoints">Name of the web listener</param>
		/// <param name="integrationOptions">Service Fabric integration options, default value is UseReverseProxyIntegration</param>
		/// <param name="kestrelOptions">Action to configure Kestrel additional, for example witch to https</param>
		/// <param name="serviceBuilder">Action to configure SF stateless service additional, for example to add more listeners or actions</param>
		/// <typeparam name="TStartup">The type containing the startup methods for the web listener</typeparam>
		public static IHost BuildStatelessWebService<TStartup>(
			this IHostBuilder hostBuilder,
			string serviceName,
			WebEndpointInfo[] endpoints,
			ServiceFabricIntegrationOptions integrationOptions = DefaultServiceFabricIntegrationOptions,
			Action<WebHostBuilderContext, KestrelServerOptions>? kestrelOptions = null,
			Action<ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>>? serviceBuilder = null)
				where TStartup : class
		{
			if (integrationOptions.HasFlag(ServiceFabricIntegrationOptions.UseUniqueServiceUrl))
			{
				// Supporting this options would require some hacks:
				// * Creating UrlSuffix like PartitionId/InstanceId where PartitionId could be taken from Env variable 'Fabric_PartitionId', for InstanceId we might thy to use new guid instead
				// * Adding this path to address returned by OmexKestrelListener
				// * In OmexServiceFabricSetupFilter initially add app.UseServiceFabricMiddleware(UrlSuffix) or create new similar middleware that with work with list of suffixes
				throw new ArgumentException("ServiceFabricIntegrationOptions.UseUniqueServiceUrl currently not supported");
			}

			return hostBuilder
				.ConfigureServices(collection =>
				{
					collection
						.AddHttpContextAccessor()
						.AddOmexMiddleware()
						.AddSingleton<IStartupFilter>(new OmexServiceFabricSetupFilter(integrationOptions));
				})
				.ConfigureWebHost(webBuilder =>
				{
					webBuilder
						.UseKestrel((WebHostBuilderContext context, KestrelServerOptions options) =>
						{
							foreach (WebEndpointInfo endpoint in endpoints)
							{
								options.Listen(IPAddress.IPv6Any, endpoint.Port, listenOptions =>
								{
									if (endpoint.UseHttps)
									{
										string certificateSubject = context.Configuration.GetValue<string>(endpoint.SettingForCertificateCommonName);
										listenOptions.UseHttps(StoreName.My, certificateSubject, true, StoreLocation.LocalMachine);
									}
								});
							}

							kestrelOptions?.Invoke(context, options);
						})
						.UseContentRoot(Directory.GetCurrentDirectory())
						.UseStartup<TStartup>()
						.UseUrls(endpoints.Select(e => e.GetListenerUrl()).ToArray());
				})
				.BuildStatelessService(
					serviceName,
					builder =>
					{
						string publisherAddress = SfConfigurationProvider.GetPublishAddress();
						foreach (WebEndpointInfo endpoint in endpoints)
						{
							builder.AddServiceListener(endpoint.Name, (p, s) =>
								new OmexKestrelListener(
									p.GetRequiredService<IServer>(),
									publisherAddress,
									endpoint.Port));
						}

						serviceBuilder?.Invoke(builder);
					});
		}
	}
}
