// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters.Settings
{
	/// <summary>
	/// The configuration settings for the IP address filter.
	/// </summary>
	public sealed class IPAddressFilterSettings
	{
		/// <summary>
		/// Gets or sets the identifier for the allowed range of IP addresses to which to provide access.
		/// </summary>
		public string AllowedRange { get; set; } = string.Empty;
	}
}
