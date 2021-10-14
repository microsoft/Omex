// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// A scrubber to remove IPv4 addresses, which can also be augmented to support the removal of additional sensitive
	/// information.
	/// </summary>
	public class IPv4AddressLogScrubber : NoOpLogScrubber
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IPv4AddressLogScrubber"/> class.
		/// </summary>
		public IPv4AddressLogScrubber() =>
			AddRule("(\\d{1,3}\\.){3}\\d{1,3}", "[IPv4 ADDRESS]");
	}
}
