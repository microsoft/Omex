
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.ServiceFabric.Client;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions
{
	/// <summary>
	/// Wrapper for Service Fabric Client
	/// </summary>
	public interface IServiceFabricClientWrapper
	{
		/// <summary>
		/// Returns stored object from wrapper
		/// </summary>
		public Task<IServiceFabricClient> GetAsync();
	}
}
