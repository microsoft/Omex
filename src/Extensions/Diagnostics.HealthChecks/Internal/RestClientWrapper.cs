﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Client;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	// TODO(4995982): Implement generic wrappers
	internal class RestClientWrapper
	{
		private IServiceFabricClient? m_client;

		private readonly Uri? m_clusterEndpoint;

		public RestClientWrapper(Uri clusterEndpoint)
		{
			// m_client is initialized during first publish async in order to avoid possible deadlocks
			m_clusterEndpoint = clusterEndpoint;
		}

		public RestClientWrapper(IServiceFabricClient client)
		{
			m_client = client;
		}

		public async Task<IServiceFabricClient> GetAsync()
		{
			if (m_client == null)
			{
				m_client = await new ServiceFabricClientBuilder()
					.UseEndpoints(m_clusterEndpoint)
					.BuildAsync()
					.ConfigureAwait(false);
			}

			return m_client;
		}
	}
}
