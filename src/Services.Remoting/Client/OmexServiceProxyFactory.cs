// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Microsoft.Omex.Extensions.Services.Remoting.Client
{
	/// <summary>
	/// Class to provide ServiceProxyFactory
	/// </summary>
	public static class OmexServiceProxyFactory
	{
		private static ServiceProxyFactory s_serviceProxyFactory = new ServiceProxyFactory(handler =>
			new OmexServiceRemotingClientFactory(
				new FabricTransportServiceRemotingClientFactory(
					remotingCallbackMessageHandler: handler)));

		/// <summary>
		/// Binds transport settings to the service proxy factory, for use in secure remoting communication.
		/// </summary>
		public static void WithTransportSettings(FabricTransportRemotingSettings transportSettings)
		{
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
		/// To use secure connections call <see cref="WithTransportSettings"/> prior to calling .Instance.
		/// </remarks>
		public static ServiceProxyFactory Instance => s_serviceProxyFactory;
	}
}
