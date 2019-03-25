// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Omex.System.Network
{
	/// <summary>
	/// Named IP Addresses
	/// </summary>
	public interface INamedIPAddresses
	{
		/// <summary>
		/// Named ranges of IP addreses
		/// </summary>
		/// <param name="rangeName">range name</param>
		/// <returns>set of ranges</returns>
		IReadOnlyCollection<IIPAddressRange> KnownIPAddressRanges(string rangeName);
	}
}