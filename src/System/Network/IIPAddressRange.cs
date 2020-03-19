// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net;

namespace Microsoft.Omex.System.Network
{
	/// <summary>
	/// Interface defining an IP Address range
	/// </summary>
	public interface IIPAddressRange
	{
		/// <summary>
		/// Friendly name
		/// </summary>
		string FriendlyName { get; }

		/// <summary>
		/// Starting address
		/// </summary>
		IPAddress StartingAddress { get; }

		/// <summary>
		/// Ending address
		/// </summary>
		IPAddress EndingAddress { get; }

		/// <summary>
		/// Is address in range
		/// </summary>
		/// <param name="address">address</param>
		/// <returns>true if in range, false otherwise</returns>
		bool IsInRange(IPAddress address);
	}
}
