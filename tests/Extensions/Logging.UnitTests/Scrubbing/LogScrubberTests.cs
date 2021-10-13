// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Logging.Scrubbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests.Scrubbing
{
	[TestClass]
	[TestCategory("Shared")]
	public class LogScrubberUnitTests
	{
		[DataTestMethod]
		[DataRow("", "")]
		[DataRow(" ", " ")]
		[DataRow("input", "input")]
		public void Scrub_WithNoRules_ShouldNotScrub(string input, string expected)
		{
			LogScrubber scrubber = LogScrubber.Instance;
			scrubber.ClearRules();

			Assert.AreEqual(expected, scrubber.Scrub(input));
		}

		[DataTestMethod]
		[DataRow("", "")]
		[DataRow(" ", " ")]
		[DataRow("input", "[REDACTED]")]
		[DataRow("inputtt", "[REDACTED]")]
		[DataRow("output", "output")]
		[DataRow("output input", "output [REDACTED]")]
		public void Scrub_WithOneRule_ShouldScrub(string input, string expected)
		{
			LogScrubber scrubber = LogScrubber.Instance;
			scrubber.ClearRules();
			scrubber.AddRule("input*", "[REDACTED]");

			Assert.AreEqual(expected, scrubber.Scrub(input));
		}

		[DataTestMethod]
		[DataRow("", "")]
		[DataRow(" ", " ")]
		[DataRow("input", "[REDACTED]")]
		[DataRow("inputtt", "[REDACTED]")]
		[DataRow("output", "output")]
		[DataRow("output input", "output [REDACTED]")]
		[DataRow("hellooo", "goodbyeoo")]
		[DataRow("hello input", "goodbye [REDACTED]")]
		[DataRow("hello hello input", "goodbye goodbye [REDACTED]")]
		public void Scrub_WithMultipleRules_ShouldScrub(string input, string expected)
		{
			LogScrubber scrubber = LogScrubber.Instance;
			scrubber.ClearRules();
			scrubber.AddRule("input*", "[REDACTED]");
			scrubber.AddRule("hello", "goodbye");

			Assert.AreEqual(expected, scrubber.Scrub(input));
		}
	}
}
