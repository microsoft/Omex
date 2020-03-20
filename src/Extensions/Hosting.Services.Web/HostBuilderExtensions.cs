// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
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
		public static ServiceFabricHostBuilder<StatelessServiceContext> AddKestrelListener<TStartup>(
			this ServiceFabricHostBuilder<StatelessServiceContext> builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null)
				where TStartup : class =>
					builder.AddKestrelListener<TStartup, StatelessServiceContext>(name, options, builderExtension);

		/// <summary>
		/// Add Kestrel service listener to stateful service
		/// </summary>
		/// <typeparam name="TStartup">The type containing the startup methods for the web listener</typeparam>
		public static ServiceFabricHostBuilder<StatefulServiceContext> AddKestrelListener<TStartup>(
			this ServiceFabricHostBuilder<StatefulServiceContext> builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null)
				where TStartup : class =>
					builder.AddKestrelListener<TStartup, StatefulServiceContext>(name, options, builderExtension);

		private static ServiceFabricHostBuilder<TContext> AddKestrelListener<TStartup, TContext>(
			this ServiceFabricHostBuilder<TContext> builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null)
				where TStartup : class
				where TContext : ServiceContext =>
					builder.AddServiceListener(new KestrelListenerBuilder<TStartup, TContext>(
						name,
						options,
						builder => BuilderExtension<TContext>(builder, builderExtension)));

		private static void BuilderExtension<TContext>(IWebHostBuilder builder, Action<IWebHostBuilder>? builderExtension)
			where TContext : ServiceContext
		{
			builderExtension?.Invoke(builder);
			builder.ConfigureServices((context, collection) => collection.AddOmexServiceFabricDependencies<TContext>());
		}
	}
}
