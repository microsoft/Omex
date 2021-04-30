
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.ServiceFabric.Client;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions
{
	/// <summary>
	/// 
	/// </summary>
	public interface IServiceFabricClientWrapper
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Task<IServiceFabricClient> GetAsync();
	}
}
