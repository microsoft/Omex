// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Microsoft.Omex.Extensions.Services.Remoting.Client
{
	/// <summary>
	/// Class to provide ServiceProxyFactory wrapper around a Service Fabric remoting client factory.
	/// </summary>
	/// <remarks>
	/// This wrapper exists to bridge the gap between native service proxy constructs and Service Fabric remoting client creation.
	/// The default behaviour is to load transport settings from the 'TransportSettings' section in the service manifest.
	/// References:
	/// https://github.com/microsoft/service-fabric-services-and-actors-dotnet/blob/master/src/Microsoft.ServiceFabric.Services.Remoting/V2/FabricTransport/Client/FabricTransportServiceRemotingClientFactory.cs#L108
	/// https://github.com/microsoft/service-fabric/blob/master/src/prod/src/managed/Microsoft.ServiceFabric.FabricTransport/FabricTransport/Common/FabricTransportSettings.cs#L300
	/// </remarks>
	public static class OmexServiceProxyFactory
	{
		private static ServiceProxyFactory s_serviceProxyFactory = new(handler =>
		{
			if (!FabricTransportRemotingSettings.TryLoadFrom("TransportSettings", out FabricTransportRemotingSettings remotingSettings))
			{
				throw new InsecureRemotingUnsupportedException();
			}

			remotingSettings.ExceptionDeserializationTechnique = FabricTransportRemotingSettings.ExceptionDeserialization.Default;
			return new OmexServiceRemotingClientFactory(
				new FabricTransportServiceRemotingClientFactory(
					remotingCallbackMessageHandler: handler,
					remotingSettings: remotingSettings));
		});

		/// <summary>
		/// Binds custom transport settings to the service proxy factory.
		/// If non-default secure remoting configuration is required, call this before calling OmexServiceProxyFactory.Instance.
		/// </summary>
		public static void WithCustomTransportSettings(FabricTransportRemotingSettings transportSettings)
		{
			transportSettings.ExceptionDeserializationTechnique = FabricTransportRemotingSettings.ExceptionDeserialization.Default;
			s_serviceProxyFactory = new ServiceProxyFactory(handler =>
				new OmexServiceRemotingClientFactory(
					new FabricTransportServiceRemotingClientFactory(
						remotingSettings: transportSettings,
						remotingCallbackMessageHandler: handler)));
		}

		/// <summary>
		/// Instance of the ServiceProxyFactory
		/// </summary>
		/// <remarks>
		/// The default implementation uses insecure connections.
		/// To use secure connections either add a TransportSettings section to the service manifest
		/// or call <see cref="WithCustomTransportSettings"/> prior to calling `.Instance` to use custom transport settings configuration.
		/// </remarks>
		public static ServiceProxyFactory Instance => s_serviceProxyFactory;
	}
}
