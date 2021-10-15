// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// A log scrubber to remove IPv4 addresses from logs.
	/// </summary>
	public class IPv4AddressLogScrubbingRule : RegexLogScrubbingRule
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IPv4AddressLogScrubbingRule"/> class.
		/// </summary>
		public IPv4AddressLogScrubbingRule() :
			base("(\\d{1,3}\\.){3}\\d{1,3}", "[IPv4 ADDRESS]")
		{
		}
	}
}
