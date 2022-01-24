// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Logging.Scrubbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests.Scrubbing
{
	[TestClass]
	[TestCategory("Shared")]
	public class RegexLogScrubbingRuleTests
	{
		[DataTestMethod]
		[DataRow("", "")]
		[DataRow(" ", " ")]
		[DataRow("input", "[REDACTED]")]
		[DataRow("Input", "[REDACTED]")]
		[DataRow("INPUT", "[REDACTED]")]
		[DataRow("iNPUT", "[REDACTED]")]
		[DataRow("inputtt", "[REDACTED]")]
		[DataRow("output", "output")]
		[DataRow("output input", "output [REDACTED]")]
		public void Scrub_ShouldScrub(string input, string expected)
		{
			RegexLogScrubbingRule scrubber = new("input*", "[REDACTED]");

			Assert.AreEqual(expected, scrubber.Scrub(input));
		}
	}
}
