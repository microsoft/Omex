// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Logging.Scrubbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests.Scrubbing
{
	[TestClass]
	[TestCategory("Shared")]
	public class IPv4AddressLogScrubbingRuleTests
	{
		[DataTestMethod]
		[DataRow("", "")]
		[DataRow(" ", " ")]
		[DataRow("input", "input")]
		[DataRow("0.0.0.0", "[IPv4 ADDRESS]")]
		[DataRow("100.100.100.100", "[IPv4 ADDRESS]")]
		[DataRow("0.0.0.0 100.100.100.100", "[IPv4 ADDRESS] [IPv4 ADDRESS]")]
		public void Scrub_ShouldScrub(string input, string expected)
		{
			IPv4AddressLogScrubbingRule scrubber = new();

			Assert.AreEqual(expected, scrubber.Scrub(input));
		}
	}
}
