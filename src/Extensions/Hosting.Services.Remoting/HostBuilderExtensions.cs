// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services.Remoting
{
	/// <summary>
	/// Extension methods for <see cref="ServiceFabricHostBuilder{TService, TContext}"/>
	/// </summary>
	public static class HostBuilderExtensions
	{
		/// <summary>
		/// Adds remote listener to statefull service
		/// </summary>
		public static ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> AddRemotingListener<TService>(
			this ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> builder,
			string name,
			FabricTransportRemotingListenerSettings? settings = null)
				where TService : class, IService
		{
			return builder
				.ConfigureServices((_, services) => services.AddTransient<TService>())
				.AddRemotingListener(name, (provider, _) => provider.GetRequiredService<TService>(), settings);
		}

		/// <summary>
		/// Adds remote listener to stateless service
		/// </summary>
		public static ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> AddRemotingListener<TService>(
			this ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> builder,
			string name,
			FabricTransportRemotingListenerSettings? settings = null)
				where TService : IService =>
					builder.AddRemotingListener(name, (provider, _) => provider.GetService<TService>(), settings);

		/// <summary>
		/// Adds remote listener to statefull service
		/// </summary>
		public static ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> AddRemotingListener<TListener>(
			this ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> builder)
				where TListener : RemotingListenerBuilder<OmexStatefulService> =>
					builder.AddServiceListener<TListener>();

		/// <summary>
		/// Adds remote listener to stateless service
		/// </summary>
		public static ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> AddRemotingListener<TListener>(
			this ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> builder)
				where TListener : RemotingListenerBuilder<OmexStatelessService> =>
					builder.AddServiceListener<TListener>();

		/// <summary>
		/// Adds remote listener using delegate to create <see cref="IService"/>
		/// </summary>
		public static ServiceFabricHostBuilder<TService, TContext> AddRemotingListener<TService, TContext>(
				this ServiceFabricHostBuilder<TService, TContext> builder,
				string name,
				Func<IServiceProvider, TService, IService> createService,
				FabricTransportRemotingListenerSettings? settings = null)
					where TService : IServiceFabricService<TContext>
					where TContext : ServiceContext =>
						builder.AddServiceListener(p =>
							new GenericRemotingListenerBuilder<TService>(name, p, createService, settings));
	}
}
