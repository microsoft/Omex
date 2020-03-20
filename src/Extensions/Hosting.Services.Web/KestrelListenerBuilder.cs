// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	/// <summary>
	/// Creates ServiceInstanceListener with all of Omex dependencies initialized
	/// </summary>
	internal sealed class KestrelListenerBuilder<TStartup,TService,TServiceContext> : IListenerBuilder<TService>
		where TStartup : class
		where TServiceContext : ServiceContext
	{
		/// <inheritdoc />
		public string Name { get; }

		/// <inheritdoc />
		public ICommunicationListener Build(TService service)
		{
			TServiceContext context = m_getContext(service);
			return new KestrelCommunicationListener(context, Name, (url, listener) => BuildWebHost(context, url, listener));
		}
			

		internal KestrelListenerBuilder(
			string name,
			ServiceFabricIntegrationOptions options,
			Func<TService,TServiceContext> getContext,
			Action<IWebHostBuilder> builderExtension)
		{
			Name = name;
			m_options = options;
			m_getContext = getContext;
			m_builderExtension = builderExtension;
		}

		// Method made internal instead of private to check test registration in service collection from unit tests
		internal IWebHost BuildWebHost(TServiceContext context, string url, AspNetCoreCommunicationListener listener)
		{
			IWebHostBuilder hostBuilder = new WebHostBuilder()
				.UseKestrel()
				.ConfigureServices(collection => ConfigureServices(context, collection))
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup<TStartup>()
				.UseServiceFabricIntegration(listener, m_options)
				.UseUrls(url)
				.UseDefaultServiceProvider(config =>
				{
					config.ValidateOnBuild = true;
					config.ValidateScopes = true;
				});

			m_builderExtension(hostBuilder);

			return hostBuilder.Build();
		}

		private void ConfigureServices(TServiceContext context, IServiceCollection services)
		{
			services.AddSingleton<ServiceContext>(context);
			services.AddSingleton(context);
		}

		private readonly ServiceFabricIntegrationOptions m_options;
		private readonly Action<IWebHostBuilder> m_builderExtension;
		private readonly Func<TService, TServiceContext> m_getContext;
	}
}
