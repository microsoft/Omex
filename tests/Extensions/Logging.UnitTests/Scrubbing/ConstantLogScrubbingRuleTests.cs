// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Logging.Scrubbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests.Scrubbing
{
	[TestClass]
	[TestCategory("Shared")]
	public class ConstantLogScrubbingRuleTests
	{
		[DataTestMethod]
		[DataRow("", "")]
		[DataRow(" ", " ")]
		[DataRow("input", "[REDACTED]")]
		[DataRow("inputtt", "[REDACTED]tt")]
		[DataRow("output", "output")]
		[DataRow("output input", "output [REDACTED]")]
		[DataRow("output input input", "output [REDACTED] [REDACTED]")]
		public void Scrub_ShouldScrub(string input, string expected)
		{
			ConstantLogScrubbingRule scrubber = new("input", "[REDACTED]");

			Assert.AreEqual(expected, scrubber.Scrub(input));
		}
	}
}
