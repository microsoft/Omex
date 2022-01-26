// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Hosting.Certificates;
using Microsoft.ServiceFabric.Client;
using Microsoft.ServiceFabric.Common.Security;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions
{
	internal class ServiceFabricClientWrapper : IServiceFabricClientWrapper
	{
		private IServiceFabricClient? m_client;

		private readonly ServiceFabricRestClientOptions m_options;

		private readonly ICertificateReader m_certificateReader;

		public ServiceFabricClientWrapper(IOptions<ServiceFabricRestClientOptions> options, ICertificateReader certificateReader)
		{
			m_options = options.Value;
			m_certificateReader = certificateReader;
		}

		public async Task<IServiceFabricClient> GetAsync(CancellationToken token)
		{
			if (m_client == null)
			{
				m_client = await CreateClientAsync(token).ConfigureAwait(false);
			}

			return m_client;
		}

		private Task<IServiceFabricClient> CreateClientAsync(CancellationToken token)
		{
			Uri clusterEndpoint = m_options.GetClusterEndpoint();

			ServiceFabricClientBuilder builder = new ServiceFabricClientBuilder().UseEndpoints(clusterEndpoint);

			if (clusterEndpoint.Scheme == Uri.UriSchemeHttps)
			{
				if (string.IsNullOrWhiteSpace(m_options.ClusterCertCommonName))
				{
					throw new InvalidOperationException($"{nameof(m_options.ClusterCertCommonName)} could not be empty for {Uri.UriSchemeHttps} cluster.");
				}

				X509Certificate2? clusterCert = m_certificateReader.GetCertificateByCommonName(m_options.ClusterCertCommonName);
				if (clusterCert == null)
				{
					throw new InvalidOperationException($"Failed to find certificate with common name '{m_options.ClusterCertCommonName}'.");
				}

				builder.UseX509Security(token =>
				{
					RemoteX509SecuritySettings remoteSecuritySettings = new(new List<X509Name> { new X509Name(clusterCert.FriendlyName) });
					return Task.FromResult<SecuritySettings>(new X509SecuritySettings(clusterCert, remoteSecuritySettings));
				});
			}

			return builder.BuildAsync(token);
		}
	}
}
