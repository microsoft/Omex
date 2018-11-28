/***************************************************************************************************
	KnownIPAddresses.cs

	Known IP Addresses
***************************************************************************************************/

using System.Web;

namespace Microsoft.Omex.Gating.Network
{
	/// <summary>
	/// Known IP Addresses
	/// </summary>
	public static class KnownIPAddressesExtensions
	{
		/// <summary>
		/// Does the request belong to a specific IP range
		/// </summary>
		/// <param name="request">request</param>
		/// <param name="knownIpAddresses">known IpAddresses</param>
		/// <param name="rangeName">range name</param>
		/// <returns>true if the request belongs to a specific range, false otherwise</returns>
		public static bool BelongsToIPRange(this HttpRequestBase request, IKnownIPAddresses knownIpAddresses, string rangeName)
		{
			if (request == null)
			{
				return false;
			}

			if (knownIpAddresses == null)
			{
				return false;
			}

			return knownIpAddresses.IsCallingFromKnownLocation(request, knownIpAddresses.KnownIPAddressRanges(rangeName));
		}
	}
}
