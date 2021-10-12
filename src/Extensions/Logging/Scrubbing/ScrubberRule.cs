// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Text.RegularExpressions;

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// Rule for Scrubber to remove sensitive information from message
	/// </summary>
	public class ScrubberRule
	{
		/// <summary>
		/// A regular expression filter specifying the data to replace
		/// </summary>
		public Regex Filter { get; }

		/// <summary>
		/// Value to replace data with
		/// </summary>
		public string ReplacementValue { get; }

		/// <summary>
		/// Constructor to create a new ScrubberRule
		/// </summary>
		/// <param name="filter">A regular expression filter specifying the data to replace</param>
		/// <param name="replacementValue">Value to replace data with</param>
		public ScrubberRule(Regex filter, string replacementValue)
		{
			Filter = filter;
			ReplacementValue = replacementValue;
		}
	}
}
