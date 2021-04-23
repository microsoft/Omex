// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.ServiceFabric.Client;
using Microsoft.ServiceFabric.Client.Http;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class RestClientWrapper
	{
		private ServiceFabricHttpClient m_client;

		private readonly Uri m_clusterEndpoint;

		private bool m_ready;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public RestClientWrapper(Uri clusterEndpoint)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		{
			// m_client is initialized during first publish async in order to avoid possible deadlocks
			m_clusterEndpoint = clusterEndpoint;
			m_ready = false;
		}

		public ServiceFabricHttpClient Get()
		{
			if (!m_ready)
			{
				m_client = (ServiceFabricHttpClient)new ServiceFabricClientBuilder()
				.UseEndpoints(m_clusterEndpoint)
				.BuildAsync().GetAwaiter().GetResult();

				m_ready = true;
			}

			return m_client;
		}

	}
}
