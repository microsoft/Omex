// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Text.RegularExpressions;

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// A log scrubber to remove sensitive information from logs in accordance with a regular expression.
	/// </summary>
	internal class RegexLogScrubbingRule : ILogScrubbingRule
	{
		private readonly Regex m_regexToReplace;
		private readonly MatchEvaluator? m_matchEvaluator;
		private readonly string? m_replacementValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="RegexLogScrubbingRule"/> class.
		/// </summary>
		/// <param name="regexToReplace">The regular expression specifying the strings to replace.</param>
		/// <param name="replacementValue">The value with which to replace the matching text.</param>
		/// <param name="matchEvaluator"></param>
		public RegexLogScrubbingRule(string regexToReplace, string? replacementValue = null, MatchEvaluator? matchEvaluator = null)
		{
			m_regexToReplace = new Regex(regexToReplace, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
			m_replacementValue = replacementValue;
			m_matchEvaluator = matchEvaluator;
		}

		/// <summary>
		/// Scrubs an input based on rules.
		/// </summary>
		/// <param name="input">The input to scrub.</param>
		/// <returns>The scrubbed input.</returns>
		public string Scrub(string input)
		{
			return m_matchEvaluator != null
				? m_regexToReplace.Replace(input, m_matchEvaluator)
				: m_regexToReplace.Replace(input, m_replacementValue ?? string.Empty);
		}
	}
}
