// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Options;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions
{
	/// <summary>
	/// Factory for service fabric client wrapper
	/// </summary>
	public class ServiceFabricClientFactory
	{
		/// <summary>
		/// Creates service fabric client wrapper instance and returns it
		/// </summary>
		/// <param name="options">REST client options</param>
		/// <returns>Service Fabric Client Wrapper</returns>
		public static IServiceFabricClientWrapper Create(ServiceFabricRestClientOptions options) => new ServiceFabricClientWrapper(Options.Create(options));
	}
}
