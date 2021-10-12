// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// Scrubber to remove sensitive information
	/// </summary>
	public interface ILogScrubber
	{
		/// <summary>
		/// Gets whether inputs should be scrubbed
		/// </summary>
		bool ShouldScrub { get; }

		/// <summary>
		/// Adds a new scrubber rule
		/// </summary>
		/// <param name="rule">Scrubber rule</param>
		void Add(ScrubberRule rule);

		/// <summary>
		/// Scrubs an input based on rules
		/// </summary>
		/// <param name="input">Input to scrub</param>
		/// <returns>Scrubbed input</returns>
		string? Scrub(string? input);
	}
}
