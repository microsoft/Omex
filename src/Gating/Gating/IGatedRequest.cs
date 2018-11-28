/**************************************************************************************************
	IGatedRequest.cs

	Interface describing a gated request.
**************************************************************************************************/

#region Using Directives

using System;
using System.Collections.Generic;
using Microsoft.Omex.Gating.Extensions;
using Microsoft.Omex.Gating.Network;

#endregion

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Interface describing a gated request.
	/// </summary>
	public interface IGatedRequest
	{
		/// <summary>
		/// The calling client (if any)
		/// </summary>
		GatedClient CallingClient { get; }


		/// <summary>
		/// The current market
		/// </summary>
		string Market { get; }


		/// <summary>
		/// The current users
		/// </summary>
		IEnumerable<GatedUser> Users { get; }


		/// <summary>
		/// Set of requested gate ids
		/// </summary>
		HashSet<string> RequestedGateIds { get; }


		/// <summary>
		/// Set of gates that are to be blocked.
		/// </summary>
		HashSet<string> BlockedGateIds { get; }


		/// <summary>
		/// The current environment
		/// </summary>
		string Environment { get; }


		/// <summary>
		/// Update the gating request based on the set of expected clients
		/// </summary>
		/// <param name="clients">expected clients</param>
		void UpdateExpectedClients(GatedClient[] clients);


		/// <summary>
		/// Gets the UserAgentBrowser from the http request.
		/// </summary>
		/// <returns>Requesting Browser and Version</returns>
		Tuple<UserAgentBrowser, int> GetUserAgentBrowser();


		/// <summary>
		/// Is the gate request part of a known ip range
		/// </summary>
		/// <param name="ipRange">ip range to check</param>
		/// <param name="knownIpAddresses">known ip addresses</param>
		bool IsPartOfKnownIPRange(IKnownIPAddresses knownIpAddresses, string ipRange);
	}
}
