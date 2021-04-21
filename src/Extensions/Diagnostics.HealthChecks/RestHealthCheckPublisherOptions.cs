// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Mtyrolski
{
	/// <summary>
	/// 
	/// </summary>
	public class RestHealthCheckPublisherOptions
	{
		/// <summary>
		/// Settings definition for Service ID
		/// </summary>
		[Required(AllowEmptyStrings = false)]
		public string RestHealthPublisherServiceId { get; set; } = string.Empty;

		/// <summary>
		/// Settings definition for Cluster Endpoint
		/// </summary>
		[Required(AllowEmptyStrings = false)]
		public string RestHealthPublisherClusterEndpoint { get; set; } = string.Empty;
	}
}
