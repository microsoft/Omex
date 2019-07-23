// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Omex.System.Network;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Base implementation of the IGateRequest interface.
	/// </summary>
	public abstract class AbstractGatedRequest : IGatedRequest
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractGatedRequest()
		{
		}


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
		/// The current query parameters
		/// </summary>
		public virtual IDictionary<string, HashSet<string>> QueryParameters { get; set; }

		/// <summary>
		/// The cloud context
		/// </summary>
		public virtual string CloudContext { get; set; }

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
		public virtual Tuple<string, int> GetUserAgentBrowser() => null;


		/// <summary>
		/// Is the gate request part of a known ip range
		/// </summary>
		/// <param name="knownIpAddresses">known ip addresses</param>
		/// <param name="ipRange">ip range to check</param>
		/// <returns>true if part of a known ip range, false otherwise</returns>
		public abstract bool IsPartOfKnownIPRange(INamedIPAddresses knownIpAddresses, string ipRange);


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
