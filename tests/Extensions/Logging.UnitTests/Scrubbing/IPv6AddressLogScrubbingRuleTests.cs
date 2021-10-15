// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Logging.Scrubbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests.Scrubbing
{
	[TestClass]
	[TestCategory("Shared")]
	public class IPv6AddressLogScrubbingRuleTests
	{
		[DataTestMethod]
		[DataRow("", "")]
		[DataRow(" ", " ")]
		[DataRow("input", "input")]
		[DataRow("1000::A01:1:AA10", "[IPv6 ADDRESS]")]
		[DataRow("1000::a01:1:aa10", "[IPv6 ADDRESS]")]
		[DataRow("1000::A01:100.0.0.0", "[IPv6 ADDRESS]")]
		[DataRow("1000::a01:100.0.0.0", "[IPv6 ADDRESS]")]
		[DataRow("1000:0:0:0:0:A01:1:AA10", "[IPv6 ADDRESS]")]
		[DataRow("1000:0:0:0:0:a01:1:aa10", "[IPv6 ADDRESS]")]
		[DataRow("1000:0:0:0:0:A01:100.0.0.0", "[IPv6 ADDRESS]")]
		[DataRow("1000:0:0:0:0:a01:100.0.0.0", "[IPv6 ADDRESS]")]
		[DataRow("1000:0:0:0:0:a01:100.0.0.0 input", "[IPv6 ADDRESS] input")]
		public void Scrub_ShouldScrub(string input, string expected)
		{
			IPv6AddressLogScrubbingRule scrubber = new();

			Assert.AreEqual(expected, scrubber.Scrub(input));
		}
	}
}
