// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// Rule for Scrubber to remove sensitive information from message
	/// </summary>
	public class ScrubberRule
	{
		/// <summary>
		/// Data to replace
		/// </summary>
		public Regex Filter { get; }

		/// <summary>
		/// Value to replace data with
		/// </summary>
		public string ReplacementValue { get; }

		/// <summary>
		/// Constructor to create a new ScrubberRule
		/// </summary>
		/// <param name="filter">Data to replace</param>
		/// <param name="replacementValue">Value to replace data with</param>
		public ScrubberRule(Regex filter, string? replacementValue = null)
		{
			Filter = Code.ExpectsArgument(filter, nameof(filter), TaggingUtilities.ReserveTag(0));
			ReplacementValue = replacementValue ?? "[REDACTED]";
		}
	}
}
