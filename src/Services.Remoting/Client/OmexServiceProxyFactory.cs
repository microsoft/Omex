// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Microsoft.Omex.Extensions.Services.Remoting.Client
{
	/// <summary>
	/// Class to provide ServiceProxyFactory
	/// </summary>
	public static class OmexServiceProxyFactory
	{
		/// <summary>
		/// Instance of the ServiceProxyFactory
		/// </summary>
		public static ServiceProxyFactory Instance { get; } = new ServiceProxyFactory(handler =>
			new OmexServiceRemotingClientFactory(
				new FabricTransportServiceRemotingClientFactory(
					remotingCallbackMessageHandler: handler)));
	}
}
