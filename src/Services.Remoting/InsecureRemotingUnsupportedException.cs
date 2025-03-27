// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Services.Remoting
{
	/// <summary>
	/// Specific exception type used when an attempt to create an insecure listener or insecure proxy is made.
	/// </summary>
	[Serializable]
	public class InsecureRemotingUnsupportedException : InvalidOperationException
	{
		/// <summary>
		/// Creates an instance of <see cref="InsecureRemotingUnsupportedException"/>.
		/// </summary>
		public InsecureRemotingUnsupportedException()
			: base("Transport security is required for Service Fabric Remoting connections. TransportSettings must be defined in settings.xml.")
		{
		}
	}
}