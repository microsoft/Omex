// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions
{
	/// <summary>
	/// Option for the REST communication with cluster
	/// </summary>
	public class ServiceFabricRestClientOptions
	{
		/// <summary>
		/// Settings definition for Cluster Endpoint Port
		/// </summary>
		public int ClusterEndpointPort { get; set; } = 19080;

		/// <summary>
		/// Settings definition for Cluster Endpoint FQDN
		/// </summary>
		[Required(AllowEmptyStrings = false)]
		public string ClusterEndpointFQDN { get; set; } = DefaultFQDN();


		/// <summary>
		/// Settings definition for Cluster Endpoint 
		/// </summary>
		public string ClusterEndpoint => string.Format("http://{0}:{1}", ClusterEndpointFQDN, ClusterEndpointPort.ToString());

		internal static string DefaultFQDN()
		{
			string? runtimeAddress = SfConfigurationProvider.GetSfVariable(RuntimeConnectionAddressEvnVariableName);
			if (runtimeAddress == null)
			{
				return string.Empty;
			}

			string[] parts = runtimeAddress.Split(':');
			if(parts.Length < 2)
			{
				return string.Empty;
			}

			return string.Join(":", parts.Take(parts.Length - 1));
		}

		internal const string RuntimeConnectionAddressEvnVariableName = "Fabric_RuntimeConnectionAddress";
	}
}
