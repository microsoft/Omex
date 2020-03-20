// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Runtime;

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
		public static ServiceFabricHostBuilder<StatelessService> AddKestrelListener<TStartup>(
			this ServiceFabricHostBuilder<StatelessService> builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null)
				where TStartup : class =>
					builder.AddKestrelListener<TStartup,StatelessService,StatelessServiceContext>(
						name,
						options,
						service => service.Context,
						builderExtension);

		/// <summary>
		/// Add Kestrel service listener to stateful service
		/// </summary>
		/// <typeparam name="TStartup">The type containing the startup methods for the web listener</typeparam>
		public static ServiceFabricHostBuilder<StatefulService> AddKestrelListener<TStartup>(
			this ServiceFabricHostBuilder<StatefulService> builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null)
				where TStartup : class =>
					builder.AddKestrelListener<TStartup,StatefulService,StatefulServiceContext>(
						name,
						options,
						service => service.Context,
						builderExtension);

		private static ServiceFabricHostBuilder<TService> AddKestrelListener<TStartup,TService,TContext>(
			this ServiceFabricHostBuilder<TService> builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Func<TService,TContext> getContext,
			Action<IWebHostBuilder>? builderExtension = null)
				where TStartup : class
				where TContext : ServiceContext =>
					builder.AddServiceListener(new KestrelListenerBuilder<TStartup,TService,TContext>(
						name,
						options,
						getContext,
						builder => builder.BuilderExtension<TContext>(builderExtension)));

		private static void BuilderExtension<TContext>(this IWebHostBuilder builder, Action<IWebHostBuilder>? builderExtension)
			where TContext : ServiceContext
		{
			builderExtension?.Invoke(builder);
			builder.ConfigureServices((context, collection) => collection.AddOmexServiceFabricDependencies<TContext>());
		}
	}
}
