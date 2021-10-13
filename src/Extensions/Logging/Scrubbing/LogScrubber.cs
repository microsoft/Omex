// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// Scrubber to remove sensitive information
	/// </summary>
	public class LogScrubber
	{
		private static readonly Lazy<LogScrubber> s_lazyInstance = new(() => new LogScrubber());

		/// <summary>
		/// The list of scrubber rules.
		/// </summary>
		/// <remarks>This is stored as a <see cref="ConcurrentBag{ScrubberRule}"/> as <see cref="LogScrubber"/> is
		/// intended to be stored as a singleton and the choice of this data structure allows for safe concurrent write
		/// access.</remarks>
		private ConcurrentBag<ScrubberRule> m_scrubberRules = new();
		
		private LogScrubber()
		{
		}

		/// <summary>
		/// The singleton instance of the PII scrubber.
		/// </summary>
		public static LogScrubber Instance =>
			s_lazyInstance.Value;

		/// <summary>
		/// Adds a new scrubber rule
		/// </summary>
		/// <param name="rule">Scrubber rule</param>
		public void AddRule(ScrubberRule rule) =>
			m_scrubberRules.Add(rule);

		/// <summary>
		/// Clears all scrubber rules.
		/// </summary>
		public void ClearRules()
		{
			ConcurrentBag<ScrubberRule> emptyScrubberRules = new();
			Interlocked.Exchange(ref m_scrubberRules, emptyScrubberRules);
		}

		/// <summary>
		/// Scrubs an input based on rules
		/// </summary>
		/// <param name="input">Input to scrub</param>
		/// <returns>Scrubbed input</returns>
		public string Scrub(string input) =>
			m_scrubberRules.Aggregate(input, (current, rule) => rule.Scrub(current));
	}
}
