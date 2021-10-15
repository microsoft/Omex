// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Text.RegularExpressions;

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// A log scrubber to remove sensitive information from logs in accordance with a regular expression.
	/// </summary>
	public class RegexLogScrubbingRule : ILogScrubbingRule
	{
		private readonly Regex m_regexToReplace;
		private readonly string m_replacementValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="RegexLogScrubbingRule"/> class.
		/// </summary>
		/// <param name="regexToReplace">The regular expression specifying the strings to replace.</param>
		/// <param name="replacementValue">The value with which to replace the matching text.</param>
		public RegexLogScrubbingRule(string regexToReplace, string replacementValue)
		{
			m_regexToReplace = new Regex(regexToReplace, RegexOptions.Compiled);
			m_replacementValue = replacementValue;
		}

		/// <summary>
		/// Scrubs an input based on rules.
		/// </summary>
		/// <param name="input">The input to scrub.</param>
		/// <returns>The scrubbed input.</returns>
		public string Scrub(string input) =>
			m_regexToReplace.Replace(input, m_replacementValue);
	}
}
