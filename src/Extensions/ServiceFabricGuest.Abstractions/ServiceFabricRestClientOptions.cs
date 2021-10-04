// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions
{
	/// <summary>
	/// Option for the REST communication with cluster
	/// </summary>
	public class ServiceFabricRestClientOptions
	{
		/// <summary>
		/// Settings definition for Cluster Endpoint Port
		/// Value taken from https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-visualizing-your-cluster
		/// </summary>
		public int ClusterEndpointPort { get; set; } = 19080;

		/// <summary>
		/// Settings definition for Cluster Endpoint FQDN
		/// </summary>
		[Required(AllowEmptyStrings = false)]
		public string ClusterEndpointFQDN { get; set; } = DefaultFQDN();

		/// <summary>
		/// Settings definition for Cluster Endpoint protocol e.g. "http" or "https"
		/// </summary>
		[Required(AllowEmptyStrings = false)]
		public string ClusterEndpointProtocol { get; set; } = Uri.UriSchemeHttp;

		/// <summary>
		/// Common name of cluster certificate, empty by default to work with unsecure cluster
		/// </summary>
		public string ClusterCertCommonName { get; set; } = string.Empty;

		/// <summary>
		/// Settings definition for Cluster Endpoint 
		/// </summary>
		internal Uri GetClusterEndpoint() => new UriBuilder(ClusterEndpointProtocol, ClusterEndpointFQDN, ClusterEndpointPort).Uri;

		internal static string DefaultFQDN()
		{
			string? runtimeAddress = SfConfigurationProvider.GetSfVariable(RuntimeConnectionAddressEvnVariableName);
			if (runtimeAddress == null)
			{
				return DefaultServiceFabricClusterFQDN;
			}

			string[] parts = runtimeAddress.Split(':');
			return parts.Length < 2 ? DefaultServiceFabricClusterFQDN : string.Join(":", parts.Take(parts.Length - 1));
		}

		internal const string RuntimeConnectionAddressEvnVariableName = "Fabric_RuntimeConnectionAddress";

		internal const string DefaultServiceFabricClusterFQDN = "localhost";
	}
}
