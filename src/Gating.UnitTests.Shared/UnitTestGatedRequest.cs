// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Omex.System.Network;

namespace Microsoft.Omex.Gating.UnitTests.Shared
{
	/// <summary>
	/// Gated request used in unit tests
	/// </summary>
	public class UnitTestGatedRequest : IGatedRequest
	{
		/// <summary>
		/// The test browser.
		/// </summary>
		private readonly string m_browser;


		/// <summary>
		/// The test browser version.
		/// </summary>
		private readonly int m_browserVersion;


		/// <summary>
		/// Construct a gated request
		/// using default values in order to register it on Instance Container
		/// </summary>
		public UnitTestGatedRequest()
			: this("BrowserThree", 10)
		{
		}


		/// <summary>
		/// Construct a gated request
		/// </summary>
		public UnitTestGatedRequest(string browser = "BrowserThree", int browserVersion = 10)
		{
			m_browser = browser;
			m_browserVersion = browserVersion;
		}


		/// <summary>
		/// The calling client (if any)
		/// </summary>
		public GatedClient CallingClient { get; set; }


		/// <summary>
		/// The current market
		/// </summary>
		public string Market { get; set; }


		/// <summary>
		/// The current user
		/// </summary>
		public IEnumerable<GatedUser> Users { get; set; }


		/// <summary>
		/// Set of requested gate ids
		/// </summary>
		public HashSet<string> RequestedGateIds { get; set; }


		/// <summary>
		/// Set of gates that are to be blocked.
		/// </summary>
		public HashSet<string> BlockedGateIds { get; set; }


		/// <summary>
		/// Environment of the gated request
		/// </summary>
		public virtual string Environment { get; set; }


		/// <summary>
		/// Query Parameters of the gated request
		/// </summary>
		public virtual IDictionary<string, HashSet<string>> QueryParameters { get; set; }

		/// <summary>
		/// Cloud contexts of the gated request
		/// </summary>
		public HashSet<string> CloudContexts { get; set; }


		/// <summary>
		/// Update the gated request based on the set of expected clients
		/// </summary>
		/// <param name="expectedClients">expected clients</param>
		public void UpdateExpectedClients(GatedClient[] expectedClients)
		{
		}


		/// <summary>
		/// Gets the UserAgentBrowser from the HttpRequest.
		/// </summary>
		/// <returns>user agent and version</returns>
		public Tuple<string, int> GetUserAgentBrowser() => m_browserVersion < 0 ? null : new Tuple<string, int>(m_browser, m_browserVersion);


		/// <summary>
		/// Is the gated request part of a named ip range
		/// </summary>
		/// <param name="namedIPAddresses">named ip addresses</param>
		/// <param name="ipRange">ip range to check</param>
		/// <returns>true if part of a named ip range, false otherwise</returns>
		public bool IsPartOfKnownIPRange(INamedIPAddresses namedIPAddresses, string ipRange) => true;
	}
}
