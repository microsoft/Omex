// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
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
		private ConcurrentBag<Tuple<Regex, string>> m_scrubberRules = new();

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
		/// <param name="regularExpression">A regular expression filter specifying the data to replace</param>
		/// <param name="replacementValue">Value to replace data with</param>
		public void AddRule(string regularExpression, string replacementValue) =>
			m_scrubberRules.Add(new Tuple<Regex, string>(new Regex(regularExpression, RegexOptions.Compiled), replacementValue));

		/// <summary>
		/// Clears all scrubber rules.
		/// </summary>
		public void ClearRules()
		{
			ConcurrentBag<Tuple<Regex, string>> emptyScrubberRules = new();
			Interlocked.Exchange(ref m_scrubberRules, emptyScrubberRules);
		}

		/// <summary>
		/// Scrubs an input based on rules
		/// </summary>
		/// <param name="input">Input to scrub</param>
		/// <returns>Scrubbed input</returns>
		public string Scrub(string input)
		{
			foreach ((Regex regularExpression, string replacementValue) in m_scrubberRules)
			{
				input = regularExpression.Replace(input, replacementValue);
			}

			return input;
		}
	}
}
