// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// Scrubber to remove sensitive information
	/// </summary>
	public class LogScrubber : ILogScrubber
	{
		private List<ScrubberRule>? m_scrubberRules;

		/// <summary>
		/// Gets whether inputs should be scrubbed
		/// </summary>
		public bool ShouldScrub => m_scrubberRules != null && m_scrubberRules.Count != 0;

		/// <summary>
		/// Adds a new scrubber rule
		/// </summary>
		/// <param name="rule">Scrubber rule</param>
		public void Add(ScrubberRule rule)
		{
			Code.ExpectsArgument(rule, nameof(rule), TaggingUtilities.ReserveTag(0));

			m_scrubberRules ??= new List<ScrubberRule>();

			m_scrubberRules.Add(rule);
		}

		/// <summary>
		/// Scrubs an input based on rules
		/// </summary>
		/// <param name="input">Input to scrub</param>
		/// <returns>Scrubbed input</returns>
		public string? Scrub(string? input)
		{
			if (m_scrubberRules == null ||
				string.IsNullOrWhiteSpace(input))
			{
				return input;
			}

			foreach (ScrubberRule rule in m_scrubberRules)
			{
				input = rule.Filter.Replace(input, rule.ReplacementValue ?? "null");
			}

			return input;
		}
	}
}
