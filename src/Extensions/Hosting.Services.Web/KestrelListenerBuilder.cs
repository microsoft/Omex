// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	/// <summary>
	/// Creates ServiceInstanceListener with all of Omex dependencies initialized
	/// </summary>
	internal sealed class KestrelListenerBuilder<TStartup,TService,TContext> : IListenerBuilder<TService>
		where TStartup : class
		where TService : IServiceFabricService<TContext>
		where TContext : ServiceContext
	{
		/// <inheritdoc />
		public string Name { get; }

		/// <inheritdoc />
		public ICommunicationListener Build(TService service) =>
			new KestrelCommunicationListener(service.Context, Name, BuildWebHost);

		internal KestrelListenerBuilder(
			string name,
			IServiceProvider serviceProvider,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder> builderExtension,
			Action<WebHostBuilderContext, KestrelServerOptions> kestrelOptions)
		{
			Name = name;
			m_serviceProvider = serviceProvider;
			m_options = options;
			m_kestrelOptions = kestrelOptions;
			m_builderExtension = builderExtension;
		}

		// Method made internal instead of private to check test registration in service collection from unit tests
		internal IWebHost BuildWebHost(string url, AspNetCoreCommunicationListener listener)
		{
			IWebHostBuilder hostBuilder = new WebHostBuilder()
				.UseKestrel(m_kestrelOptions)
				.ConfigureServices(collection => ConfigureServices(collection))
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

		private void ConfigureServices(IServiceCollection services)
		{
			services.AddOmexMiddleware()
				.PropagateRequired<Accessor<TContext>>(m_serviceProvider)
				.PropagateOptional<Accessor<IReliableStateManager>>(m_serviceProvider)
				.PropagateOptional<Accessor<IStatefulServicePartition>>(m_serviceProvider)
				.PropagateOptional<Accessor<IStatelessServicePartition>>(m_serviceProvider);
		}

		private readonly IServiceProvider m_serviceProvider;

		private readonly ServiceFabricIntegrationOptions m_options;

		private readonly Action<WebHostBuilderContext, KestrelServerOptions> m_kestrelOptions;

		private readonly Action<IWebHostBuilder> m_builderExtension;
	}
}
