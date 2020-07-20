﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	/// <summary>
	/// Extension to add Omex dependencies to HostBuilder
	/// </summary>
	public static class HostBuilderExtensions
	{
		/// <summary>
		/// Add Kestrel service listener to stateless service
		/// </summary>
		/// <typeparam name="TStartup">The type containing the startup methods for the web listener</typeparam>
		public static ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> AddKestrelListener<TStartup>(
			this ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null,
			Action<WebHostBuilderContext, KestrelServerOptions>? kestrelOptions = null)
				where TStartup : class =>
					builder.AddKestrelListener<TStartup, OmexStatelessService, StatelessServiceContext>(
						name,
						options,
						builderExtension,
						kestrelOptions);

		/// <summary>
		/// Add Kestrel service listener to stateful service
		/// </summary>
		/// <typeparam name="TStartup">The type containing the startup methods for the web listener</typeparam>
		public static ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> AddKestrelListener<TStartup>(
			this ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null,
			Action<WebHostBuilderContext, KestrelServerOptions>? kestrelOptions = null)
				where TStartup : class =>
					builder.AddKestrelListener<TStartup, OmexStatefulService, StatefulServiceContext>(
						name,
						options,
						builderExtension,
						kestrelOptions);

		private static ServiceFabricHostBuilder<TService, TContext> AddKestrelListener<TStartup, TService, TContext>(
			this ServiceFabricHostBuilder<TService, TContext> builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null,
			Action<WebHostBuilderContext, KestrelServerOptions>? kestrelOptions = null)
				where TStartup : class
				where TService : IServiceFabricService<TContext>
				where TContext : ServiceContext =>
					builder.AddServiceListener(provider => new KestrelListenerBuilder<TStartup, TService, TContext>(
						name,
						provider,
						options,
						builder => builder.BuilderExtension<TContext>(builderExtension),
						kestrelOptions ??= s_emptyKestrelOptions));

		private static readonly Action<WebHostBuilderContext, KestrelServerOptions> s_emptyKestrelOptions = (webHostBuilderContext, kestrelServerOptions) => { };

		private static void BuilderExtension<TContext>(this IWebHostBuilder builder, Action<IWebHostBuilder>? builderExtension)
			where TContext : ServiceContext
		{
			builderExtension?.Invoke(builder);
			builder.ConfigureServices((context, collection) => collection.AddOmexServiceFabricDependencies<TContext>());
		}
	}
}
