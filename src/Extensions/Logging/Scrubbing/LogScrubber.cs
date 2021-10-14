// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// Scrubber to remove sensitive information
	/// </summary>
	public class LogScrubber
	{
		private static readonly Lazy<LogScrubber> s_lazyInstance = new(() => new LogScrubber());

		private readonly List<Tuple<Regex, string>> m_scrubberRules = new();

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
		/// Adds a rule to scrub IPv4 addresses.
		/// </summary>
		public void AddIPv4AddressRule() =>
			AddRule("(\\d{1,3}\\.){3}\\d{1,3}", "[IPv4 ADDRESS]");

		/// <summary>
		/// Clears all scrubber rules.
		/// </summary>
		public void ClearRules() =>
			m_scrubberRules.Clear();

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
