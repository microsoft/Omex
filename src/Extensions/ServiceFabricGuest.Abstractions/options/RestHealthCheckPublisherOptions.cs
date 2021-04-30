// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions
{
	/// <summary>
	/// Option for the REST Health publisher
	/// </summary>
	public class RestHealthCheckPublisherOptions
	{
		/// <summary>
		/// Settings definition for Cluster Endpoint
		/// </summary>
		[Required(AllowEmptyStrings = false)]
		public string RestHealthPublisherClusterEndpoint { get; set; } = string.Empty;
	}
}
