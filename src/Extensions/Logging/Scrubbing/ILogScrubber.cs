// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// An interface to a scrubber to remove sensitive information.
	/// </summary>
	public interface ILogScrubber
	{
		/// <summary>
		/// Scrubs an input based on rules.
		/// </summary>
		/// <param name="input">The input to scrub.</param>
		/// <returns>The scrubbed input.</returns>
		string Scrub(string input);
	}
}
