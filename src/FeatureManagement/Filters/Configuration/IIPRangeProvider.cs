// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net;

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters.Configuration
{
	/// <summary>
	/// Provides IP address ranges for named groups.
	/// </summary>
	public interface IIPRangeProvider
	{
		/// <summary>
		/// Gets the IP address ranges for the specified range name.
		/// </summary>
		/// <param name="rangeName">The name of the IP range group.</param>
		/// <returns>An array of IP networks for the specified range name, or <c>null</c> if the range name is not found.</returns>
		IPNetwork[]? GetIPRanges(string rangeName);
	}
}
