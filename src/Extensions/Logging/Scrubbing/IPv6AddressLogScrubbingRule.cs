// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// A log scrubber to remove IPv6 addresses from logs.
	/// </summary>
	public class IPv6AddressLogScrubbingRule : RegexLogScrubbingRule
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IPv6AddressLogScrubbingRule"/> class.
		/// </summary>
		public IPv6AddressLogScrubbingRule() :
			base(
				"(?:(?:(?:[a-fA-F0-9]{1,4}:){6}|(?=(?:[a-fA-F0-9]{0,4}:){0,6}(?:[0-9]{1,3}\\.){3}[0-9]{1,3}(?![:.\\w]))(([a-fA-F0-9]{1,4}:){0,5}|:)((:[a-fA-F0-9]{1,4}){1,5}:|:)|::(?:[a-fA-F0-9]{1,4}:){5})(?:(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|(?:[a-fA-F0-9]{1,4}:){7}[a-fA-F0-9]{1,4}|(?=(?:[a-fA-F0-9]{0,4}:){0,7}[a-fA-F0-9]{0,4}(?![:.\\w]))(([a-fA-F0-9]{1,4}:){1,7}|:)((:[a-fA-F0-9]{1,4}){1,7}|:)|(?:[a-fA-F0-9]{1,4}:){7}:|:(:[a-fA-F0-9]{1,4}){7})(?![:.\\w])",
				"[IPv6 ADDRESS]")
		{
		}
	}
}
