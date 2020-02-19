// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Fabric.Description;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.ServiceFabric.Services
{
	/// <summary>
	/// Creates ServiceInstanceListener with all of Omex dependencies inicialized
	/// </summary>
	public class KestrelListenerBuilder<TServiceContext> : IListenerBuilder<TServiceContext>
		where TServiceContext : ServiceContext
	{
		/// <inheridoc />
		public string Name { get; }


		/// <inheridoc />
		public ICommunicationListener Build(TServiceContext context) =>
			new KestrelCommunicationListener(context, Name, (url, listener) => BuildWebHost(context, url, listener));


		internal KestrelListenerBuilder(
			Type startupType,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension)
		{
			Name = name;
			m_startupType = startupType;
			m_options = options;
			m_builderExtension = builderExtension;
		}


		private readonly Type m_startupType;
		private readonly ServiceFabricIntegrationOptions m_options;
		private readonly Action<IWebHostBuilder>? m_builderExtension;


		private void ConfigureServices(TServiceContext collection, IServiceCollection services)
		{
			services.AddSingleton<ServiceContext>(collection);
			services.AddSingleton(collection);
		}


		private IWebHost BuildWebHost(TServiceContext context, string url, AspNetCoreCommunicationListener listener)
		{
			EndpointResourceDescription endpointConfig = context.CodePackageActivationContext.GetEndpoint(Name);
			url += endpointConfig.PathSuffix;

			IWebHostBuilder hostBuilder = new WebHostBuilder()
				.UseKestrel()
				.ConfigureServices(collection => ConfigureServices(context, collection))
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup(m_startupType)
				.UseServiceFabricIntegration(listener, m_options)
				.UseUrls(url)
				.UseDefaultServiceProvider(config =>
				{
					config.ValidateOnBuild = true;
					config.ValidateScopes = true;
				});

			m_builderExtension?.Invoke(hostBuilder);

			return hostBuilder.Build();
		}
	}
}
