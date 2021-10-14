// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Abstractions.Scrubbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Activities.UnitTests.Scrubbing
{
	[TestClass]
	[TestCategory("Shared")]
	public class NoOpTextScrubberUnitTests
	{
		[DataTestMethod]
		[DataRow("", "")]
		[DataRow(" ", " ")]
		[DataRow("input", "input")]
		public void Scrub_WithNoRules_ShouldNotScrub(string input, string expected)
		{
			NoOpTextScrubber scrubber = new();

			Assert.AreEqual(expected, scrubber.Scrub(input));
		}

		[DataTestMethod]
		[DataRow("", "")]
		[DataRow(" ", " ")]
		[DataRow("input", "input")]
		public void Scrub_WithInheritedScrubberAndNoRules_ShouldNotScrub(string input, string expected)
		{
			InheritedTextScrubberTests scrubber = new();

			Assert.AreEqual(expected, scrubber.Scrub(input));
		}

		[DataTestMethod]
		[DataRow("", "")]
		[DataRow(" ", " ")]
		[DataRow("input", "[REDACTED]")]
		[DataRow("inputtt", "[REDACTED]")]
		[DataRow("output", "output")]
		[DataRow("output input", "output [REDACTED]")]
		public void Scrub_WithInheritedScrubberAndOneRule_ShouldScrub(string input, string expected)
		{
			InheritedTextScrubberTests scrubber = new();
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
		public void Scrub_WithInheritedScrubberAndMultipleRules_ShouldScrub(string input, string expected)
		{
			InheritedTextScrubberTests scrubber = new();
			scrubber.AddRule("input*", "[REDACTED]");
			scrubber.AddRule("hello", "goodbye");

			Assert.AreEqual(expected, scrubber.Scrub(input));
		}

		private class InheritedTextScrubberTests : NoOpTextScrubber
		{
			public new void AddRule(string regularExpression, string replacementValue) =>
				base.AddRule(regularExpression, replacementValue);
		}
	}
}
