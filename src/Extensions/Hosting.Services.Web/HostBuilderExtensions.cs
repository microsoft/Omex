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
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
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
		/// <param name="webEndpointName">Name of the web listener</param>
		/// <param name="options">Service Fabric integration options, default value is UseReverseProxyIntegration</param>
		/// <param name="kestrelOptions">Action to configure Kestrel additional, for example witch to https</param>
		/// <param name="serviceBuilder">Action to configure SF stateless service additional, for example to add more listeners or actions</param>
		/// <typeparam name="TStartup">The type containing the startup methods for the web listener</typeparam>
		internal static IHost BuildStatelessHttpService<TStartup>(
			IHostBuilder hostBuilder,
			string serviceName,
			string webEndpointName,
			ServiceFabricIntegrationOptions options = DefaultServiceFabricIntegrationOptions,
			Action<WebHostBuilderContext, KestrelServerOptions>? kestrelOptions = null,
			Action<ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>>? serviceBuilder = null)
				where TStartup : class =>
					BuildStatelessWebService<TStartup>(
						hostBuilder,
						serviceName,
						webEndpointName,
						options,
						certificateCommonNameForHttps: null,
						kestrelOptions,
						serviceBuilder);

		/// <summary>
		/// Build stateless service with Kestrel service listener
		/// </summary>
		/// <param name="hostBuilder">Host builder</param>
		/// <param name="serviceName">Name of Service Fabric stateless service</param>
		/// <param name="webEndpointName">Name of the web listener</param>
		/// <param name="options">Service Fabric integration options, default value is UseReverseProxyIntegration</param>
		/// <param name="certificateCommonNameForHttps">Name of the setting to get certificate common name for https, default value is 'Certificates:SslCertificateCommonName'</param>
		/// <param name="kestrelOptions">Action to configure Kestrel additional, for example witch to https</param>
		/// <param name="serviceBuilder">Action to configure SF stateless service additional, for example to add more listeners or actions</param>
		/// <typeparam name="TStartup">The type containing the startup methods for the web listener</typeparam>
		internal static IHost BuildStatelessHttpsService<TStartup>(
			IHostBuilder hostBuilder,
			string serviceName,
			string webEndpointName,
			ServiceFabricIntegrationOptions options = DefaultServiceFabricIntegrationOptions,
			string? certificateCommonNameForHttps = "Certificates:SslCertificateCommonName",
			Action<WebHostBuilderContext, KestrelServerOptions>? kestrelOptions = null,
			Action<ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>>? serviceBuilder = null)
				where TStartup : class =>
					BuildStatelessWebService<TStartup>(
						hostBuilder,
						serviceName,
						webEndpointName,
						options,
						certificateCommonNameForHttps,
						kestrelOptions,
						serviceBuilder);

		internal static IHost BuildStatelessWebService<TStartup>(
			IHostBuilder hostBuilder,
			string serviceName,
			string webEndpointName,
			ServiceFabricIntegrationOptions options,
			string? certificateCommonNameForHttps,
			Action<WebHostBuilderContext, KestrelServerOptions>? kestrelOptions,
			Action<ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>>? serviceBuilder)
				where TStartup : class
		{
			WebListenerConfiguration sfConfig = new WebListenerConfiguration(webEndpointName, options, certificateCommonNameForHttps);

			return hostBuilder
				.ConfigureServices(collection =>
				{
					collection
						.AddHttpContextAccessor()
						.AddOmexMiddleware()
						.AddSingleton<IStartupFilter>(new OmexServiceFabricSetupFilter(sfConfig.UrlSuffix, options));
				})
				.ConfigureWebHost(webBuilder =>
				{
					webBuilder
						.UseKestrel((WebHostBuilderContext context, KestrelServerOptions options) =>
						{
							if (sfConfig.UseHttps)
							{
								options.Listen(IPAddress.IPv6Any, sfConfig.EndpointPort, listenOptions =>
								{
									string certificateSubject = context.Configuration.GetValue<string>(sfConfig.CertificateCommonNameForHttps);
									listenOptions.UseHttps(StoreName.My, certificateSubject, true, StoreLocation.LocalMachine);
								});
							}

							kestrelOptions?.Invoke(context, options);
						})
						.UseContentRoot(Directory.GetCurrentDirectory())
						.UseStartup<TStartup>()
						.UseUrls(sfConfig.ListenerUrl);
				})
				.BuildStatelessService(
					serviceName,
					builder =>
					{
						builder.AddServiceListener(webEndpointName, (p, s) => new OmexKestrelListener(p.GetRequiredService<IServer>(), sfConfig));
						serviceBuilder?.Invoke(builder);
					});
		}
	}
}
