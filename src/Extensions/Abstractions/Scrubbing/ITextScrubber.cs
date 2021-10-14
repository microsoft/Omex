// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Abstractions.Scrubbing
{
	/// <summary>
	/// An interface to a text scrubber to remove sensitive information.
	/// </summary>
	public interface ITextScrubber
	{
		/// <summary>
		/// Scrubs an input based on rules.
		/// </summary>
		/// <param name="input">The input to scrub.</param>
		/// <returns>The scrubbed input.</returns>
		string Scrub(string input);
	}
}
