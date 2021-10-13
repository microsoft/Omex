// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.Omex.Extensions.Logging.Scrubbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests.Scrubbing
{
	[TestClass]
	[TestCategory("Shared")]
	public class ScrubberRuleUnitTests
	{
		[TestMethod]
		public void Add_WithValues_StoresValues()
		{
			Regex filter = new("filter");
			const string replacementValue = "replacementValue";
			ScrubberRule scrubberRule = new(filter, replacementValue);

			Assert.AreEqual(filter, scrubberRule.Filter);
			Assert.AreEqual(replacementValue, scrubberRule.ReplacementValue);
		}

		[DataTestMethod]
		[DataRow("", "")]
		[DataRow(" ", " ")]
		[DataRow("input", "[REDACTED]")]
		[DataRow("inputtt", "[REDACTED]")]
		[DataRow("output", "output")]
		[DataRow("output input", "output [REDACTED]")]
		public void Scrub_WhenCalled_ShouldScrub(string input, string expected)
		{
			ScrubberRule scrubberRule = new(new Regex("input*"), "[REDACTED]");

			Assert.AreEqual(expected, scrubberRule.Scrub(input));
		}

		[TestMethod]
		public void Scrub_WhenCalledWithInputContainingMultipleSubstringsToReplace_ShouldScrub()
		{
			ScrubberRule scrubberRule = new(new Regex("input"), "[REDACTED]");

			Assert.AreEqual("[REDACTED] [REDACTED] output [REDACTED]", scrubberRule.Scrub("input input output input"));
		}
	}
}
