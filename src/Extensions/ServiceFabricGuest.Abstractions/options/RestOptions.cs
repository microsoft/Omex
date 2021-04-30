// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions
{
	/// <summary>
	/// Option for the REST communication with cluster
	/// </summary>
	public class RestOptions
	{
		/// <summary>
		/// Settings definition for Cluster Endpoint
		/// </summary>
		[Required(AllowEmptyStrings = false)]
		public string ClusterEndpoint { get; set; } = string.Empty;
	}
}
