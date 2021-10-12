// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// Scrubber to remove sensitive information
	/// </summary>
	public class LogScrubber : ILogScrubber
	{
		private readonly List<ScrubberRule> m_scrubberRules = new();

		/// <summary>
		/// Adds a new scrubber rule
		/// </summary>
		/// <param name="rule">Scrubber rule</param>
		public void AddRule(ScrubberRule rule) =>
			m_scrubberRules.Add(rule);

		/// <summary>
		/// Scrubs an input based on rules
		/// </summary>
		/// <param name="input">Input to scrub</param>
		/// <returns>Scrubbed input</returns>
		public string Scrub(string input) =>
			m_scrubberRules.Aggregate(input, (current, rule) => rule.Filter.Replace(current, rule.ReplacementValue));
	}
}
