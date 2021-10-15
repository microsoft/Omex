// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// A log scrubber to remove sensitive information from logs in accordance with a constant value.
	/// </summary>
	public class ConstantLogScrubbingRule : ILogScrubbingRule
	{
		private readonly string m_valueToReplace;
		private readonly string m_replacementValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstantLogScrubbingRule"/> class.
		/// </summary>
		/// <param name="valueToReplace">The value to be replaced.</param>
		/// <param name="replacementValue">The value with which to replace the matching text.</param>
		public ConstantLogScrubbingRule(string valueToReplace, string replacementValue)
		{
			m_valueToReplace = valueToReplace;
			m_replacementValue = replacementValue;
		}

		/// <summary>
		/// Scrubs an input based on rules.
		/// </summary>
		/// <param name="input">The input to scrub.</param>
		/// <returns>The scrubbed input.</returns>
		public string Scrub(string input) =>
			input.Replace(m_valueToReplace, m_replacementValue);
	}
}
