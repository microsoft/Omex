// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.Omex.Extensions.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services.Remoting
{
	/// <summary>
	/// Base builder for <see cref="FabricTransportServiceRemotingListener"/>
	/// </summary>
	public abstract class RemotingListenerBuilder<TService> : IListenerBuilder<TService>
		where TService : IServiceFabricService<ServiceContext>
	{
		private readonly FabricTransportRemotingListenerSettings? m_settings;

		/// <summary>
		/// Constructor
		/// </summary>
		protected RemotingListenerBuilder(
			string name,
			FabricTransportRemotingListenerSettings? settings = null)
		{
			Name = name;
			m_settings = settings;
		}

		/// <summary>
		/// Method should create <see cref="IService"/> for <see cref="FabricTransportServiceRemotingListener"/>
		/// </summary>
		public abstract IService BuildService(TService service);

		/// <inheritdoc />
		public string Name { get; }

		ICommunicationListener IListenerBuilder<TService>.Build(TService service)
		{
			ServiceContext context = service.Context;
			return new FabricTransportServiceRemotingListener(
				context,
				new OmexServiceRemotingDispatcher(context, BuildService(service)),
				m_settings);
		}
	}
}
