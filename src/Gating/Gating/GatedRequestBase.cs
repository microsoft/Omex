/***************************************************************************
	GatedRequestBase.cs

	Base implementation of the IGatedRequest interface.
***************************************************************************/

using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.Omex.Gating.Extensions;
using Microsoft.Omex.Gating.Network;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Base implementation of the IGateRequest interface.
	/// </summary>
	public abstract class GatedRequestBase : IGatedRequest
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		protected GatedRequestBase()
		{
		}


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="request">The request.</param>
		protected GatedRequestBase(HttpRequestBase request)
		{
			Request = Code.ExpectsArgument(request, nameof(request), TaggingUtilities.ReserveTag(0x23850602 /* tag_97qyc */));
		}


		/// <summary>
		/// Current Http request
		/// </summary>
		protected HttpRequestBase Request { get; set; }


		/// <summary>
		/// The calling client (if any)
		/// </summary>
		public virtual GatedClient CallingClient { get; set; }


		/// <summary>
		/// The current market
		/// </summary>
		public virtual string Market { get; set; }


		/// <summary>
		/// Current users
		/// </summary>
		public virtual IEnumerable<GatedUser> Users { get; set; }


		/// <summary>
		/// The current environment
		/// </summary>
		public virtual string Environment { get; set; }


		/// <summary>
		/// Update the gate request based on the set of expected clients
		/// </summary>
		/// <param name="clients">clients</param>
		public virtual void UpdateExpectedClients(GatedClient[] clients)
		{
		}


		/// <summary>
		/// Gets the browser's type and browser's version.
		/// </summary>
		/// <returns>A Tuple(pair) of Browser and its version.</returns>
		public virtual Tuple<UserAgentBrowser, int> GetUserAgentBrowser() => null;


		/// <summary>
		/// Is the gate request part of a known ip range
		/// </summary>
		/// <param name="knownIpAddresses">known ip addresses</param>
		/// <param name="ipRange">ip range to check</param>
		/// <returns>true if part of a known ip range, false otherwise</returns>
		public virtual bool IsPartOfKnownIPRange(IKnownIPAddresses knownIpAddresses, string ipRange)
		{
			if (!Code.ValidateNotNullOrWhiteSpaceArgument(ipRange, nameof(ipRange), TaggingUtilities.ReserveTag(0x23850603 /* tag_97qyd */)))
			{
				return false;
			}

			if (m_ipRanges.TryGetValue(ipRange, out bool isPartOfIPAddress))
			{
				return isPartOfIPAddress;
			}

			isPartOfIPAddress = Request?.BelongsToIPRange(knownIpAddresses, ipRange) ?? false;
			m_ipRanges[ipRange] = isPartOfIPAddress;
			return isPartOfIPAddress;
		}


		/// <summary>
		/// Set of requested gate ids
		/// </summary>
		public virtual HashSet<string> RequestedGateIds { get; set; }


		/// <summary>
		/// Set of blocked gate ids.
		/// </summary>
		public virtual HashSet<string> BlockedGateIds { get; set; }


		/// <summary>
		/// Dictionary of ip ranges the request is part of
		/// </summary>
		private Dictionary<string, bool> m_ipRanges = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
	}
}
