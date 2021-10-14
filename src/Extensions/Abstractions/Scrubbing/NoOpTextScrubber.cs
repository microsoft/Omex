// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Omex.Extensions.Abstractions.Scrubbing
{
	/// <summary>
	/// A no-op scrubber to remove sensitive information, which can be subclassed to support the removal of sensitive
	/// information.
	/// </summary>
	public class NoOpTextScrubber : ITextScrubber
	{
		private readonly List<Tuple<Regex, string>> m_scrubberRules = new();

		/// <summary>
		/// Adds a new scrubber rule.
		/// </summary>
		/// <param name="regularExpression">A regular expression filter specifying the data to replace.</param>
		/// <param name="replacementValue">The value with which to replace the data.</param>
		protected void AddRule(string regularExpression, string replacementValue) =>
			m_scrubberRules.Add(new Tuple<Regex, string>(new Regex(regularExpression, RegexOptions.Compiled), replacementValue));

		/// <summary>
		/// Scrubs an input based on rules.
		/// </summary>
		/// <param name="input">The input to scrub.</param>
		/// <returns>The scrubbed input.</returns>
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
