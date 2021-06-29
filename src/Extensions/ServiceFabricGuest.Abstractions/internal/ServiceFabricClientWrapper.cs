// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Client;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions
{
	internal class ServiceFabricClientWrapper : IServiceFabricClientWrapper
	{
		private IServiceFabricClient? m_client;

		private Uri m_clusterEndpoint;

		public ServiceFabricClientWrapper(IOptions<ServiceFabricRestClientOptions> options) => m_clusterEndpoint = new(options.Value.ClusterEndpoint());

		public async Task<IServiceFabricClient> GetAsync(CancellationToken token)
		{
			if (m_client == null)
			{
				m_client = await new ServiceFabricClientBuilder()
					.UseEndpoints(m_clusterEndpoint)
					.BuildAsync(token)
					.ConfigureAwait(false);
			}

			return m_client;
		}
	}
}
