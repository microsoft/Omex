// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services.Remoting
{
	/// <summary>
	/// Extnsion methods for <see cref="ServiceFabricHostBuilder{TService, TContext}"/>
	/// </summary>
	public static class HostBuilderExtensions
	{
		/// <summary>
		/// Add's remote listener to statefull service
		/// </summary>
		public static ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> AddRemotingListener<TListener>(
			this ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> builder)
				where TListener : RemotingListenerBuilder<OmexStatefulService> =>
					builder.AddServiceListener<TListener>();

		/// <summary>
		/// Add's remote listener to stateless service
		/// </summary>
		public static ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> AddRemotingListener<TListener>(
			this ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> builder)
				where TListener : RemotingListenerBuilder<OmexStatelessService> =>
					builder.AddServiceListener<TListener>();

		/// <summary>
		/// Add's remote listener using delegate to create <see cref="IService"/>
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
