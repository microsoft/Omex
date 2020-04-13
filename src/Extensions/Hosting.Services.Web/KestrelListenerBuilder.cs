// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	/// <summary>
	/// Creates ServiceInstanceListener with all of Omex dependencies initialized
	/// </summary>
	internal sealed class KestrelListenerBuilder<TStartup, TService, TContext> : IListenerBuilder<TContext>
		where TStartup : class
		where TService : class, IServiceFabricService<TContext>
		where TContext : ServiceContext
	{
		/// <inheritdoc />
		public string Name { get; }

		/// <inheritdoc />
		public ICommunicationListener Build(TContext context) =>
			new KestrelCommunicationListener(context, Name, (url, listener) => BuildWebHost(context, url, listener));

		internal KestrelListenerBuilder(
			string name,
			IServiceProvider serviceProvider,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder> builderExtension)
		{
			Name = name;
			m_serviceProvider = serviceProvider;
			m_options = options;
			m_builderExtension = builderExtension;
		}

		// Method made internal instead of private to check test registration in service collection from unit tests
		internal IWebHost BuildWebHost(TContext context, string url, AspNetCoreCommunicationListener listener)
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

		private void ConfigureServices(TContext context, IServiceCollection collection)
		{
			collection.AddSingleton<ServiceContext>(context);
			collection.AddSingleton(context);
			PropagateAccessors(collection, m_serviceProvider);
			AddMiddlewares(collection);
		}

		private readonly IServiceProvider m_serviceProvider;

		private readonly ServiceFabricIntegrationOptions m_options;

		private readonly Action<IWebHostBuilder> m_builderExtension;

		private static void PropagateAccessors(IServiceCollection collection, IServiceProvider provider)
		{
			collection
				.AddSingleton(provider.GetRequiredService<IAccessor<TService>>())
				.AddSingleton(provider.GetRequiredService<IAccessor<TContext>>());

			IAccessor<IReliableStateManager>? accessor = provider.GetService<IAccessor<IReliableStateManager>>();
			if (accessor == null)
			{
				collection.AddSingleton(accessor);
			}
		}

		private static void AddMiddlewares(IServiceCollection collection)
		{
			collection
				.AddSingleton<ActivityEnrichmentMiddleware>()
				.AddSingleton<ResponseHeadersMiddleware>();

#pragma warning disable CS0618 // We need to register all middlewares, even if obsolete
			collection.AddSingleton<ObsoleteCorrelationHeadersMiddleware>();
#pragma warning restore CS0618
		}
	}
}
