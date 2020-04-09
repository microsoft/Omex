// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services.Remoting
{
	/// <summary>
	/// Static methods to simplify creation of <see cref="FabricTransportRemotingListenerSettings" />
	/// </summary>
	public static class RemotingSettings
	{
		/// <summary>
		/// Creates <see cref="FabricTransportRemotingListenerSettings" /> with provided EndpointResourceName
		/// </summary>
		public static FabricTransportRemotingListenerSettings WithEndpoint(string value)
		{
			return new FabricTransportRemotingListenerSettings()
			{
				EndpointResourceName = value
			};
		}
	}
}
