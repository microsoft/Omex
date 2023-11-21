// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
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
		/// <inheritdoc />
		public string Name { get; }

		// done internal for unit tests
		internal FabricTransportRemotingListenerSettings Settings { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>Transport security settings should be defined in TransportSettings of a service's settings.xml before creating a listener</remarks>
		protected RemotingListenerBuilder(
			string name,
			FabricTransportRemotingListenerSettings? settings = null)
		{
			if (settings == null && !FabricTransportRemotingListenerSettings.TryLoadFrom("TransportSettings", out settings))
			{
				throw new InvalidOperationException("Transport security is required for Service Fabric Remoting connections. TransportSettings must be defined in settings.xml.");
			}

			settings.ExceptionSerializationTechnique = FabricTransportRemotingListenerSettings.ExceptionSerialization.Default;
			Name = name;
			Settings = settings;
		}

		/// <summary>
		/// Method should create <see cref="IService"/> for <see cref="FabricTransportServiceRemotingListener"/>
		/// </summary>
		public abstract IService BuildService(TService service);

		ICommunicationListener IListenerBuilder<TService>.Build(TService service)
		{
			ServiceContext context = service.Context;
			return new FabricTransportServiceRemotingListener(
				context,
				new OmexServiceRemotingDispatcher(context, BuildService(service)),
				Settings);
		}
	}
}
