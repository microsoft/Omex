/***************************************************************************************************
	IKnownIPAddresses.cs

	Known IP Addresses
***************************************************************************************************/

using System.Collections.Generic;
using System.Web;
using Microsoft.Omex.System.Network;

namespace Microsoft.Omex.Gating.Network
{
	/// <summary>
	/// Known IP Addresses
	/// </summary>
	public interface IKnownIPAddresses
	{
		/// <summary>
		/// Known ranges of IP addreses
		/// </summary>
		/// <param name="rangeName">range name</param>
		/// <returns>set of ranges</returns>
		IReadOnlyCollection<IIPAddressRange> KnownIPAddressRanges(string rangeName);


		/// <summary>
		/// Non-routable addresses
		/// </summary>
		IReadOnlyCollection<IIPAddressRange> NonRoutableAddresses { get; }


		/// <summary>
		/// Is calling from a known location
		/// </summary>
		/// <param name="request">request</param>
		/// <param name="ranges">set of known ranges</param>
		/// <returns>true if calling from a known location, false otherwise</returns>
		bool IsCallingFromKnownLocation(HttpRequestBase request, IEnumerable<IIPAddressRange> ranges);
	}
}
