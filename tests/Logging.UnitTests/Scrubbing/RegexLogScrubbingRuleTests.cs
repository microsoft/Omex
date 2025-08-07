// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.Omex.Extensions.Logging.Scrubbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests.Scrubbing
{
	[TestClass]
	[TestCategory("Shared")]
	public class RegexLogScrubbingRuleTests
	{
		[TestMethod]
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

		[TestMethod]
		[DataRow("/api/path?q1=v1&q2=v2&q3=v3", "/api/path?REDACTED&REDACTED&q3=v3")]
		[DataRow("/api/path?q1=v1&q2=v2", "/api/path?REDACTED&REDACTED&")]
		[DataRow("/api/path?q3=v3", "/api/path?q3=v3")]
		public void Scrub_ShouldScrub_WithoutMatchEvaluator(string input, string expected)
		{
			RegexLogScrubbingRule scrubber = new("(q1|q2)=(.+?)(&|$)", "REDACTED&");

			Assert.AreEqual(expected, scrubber.Scrub(input));
		}

		[TestMethod]
		[DataRow("/api/path?q1=v1&q2=v2&q3=v3", "/api/path?q1=REDACTED&q2=REDACTED&q3=v3")]
		[DataRow("/api/path?q1=v1&q2=v2", "/api/path?q1=REDACTED&q2=REDACTED")]
		[DataRow("/api/path?q3=v3", "/api/path?q3=v3")]
		public void Scrub_ShouldScrub_WithMatchEvaluator(string input, string expected)
		{
			MatchEvaluator matchEvaluator = new((match) => match.Groups[1].Value + "=REDACTED" + match.Groups[3].Value);
			RegexLogScrubbingRule scrubber = new("(q1|q2)=(.+?)(&|$)", matchEvaluator);

			Assert.AreEqual(expected, scrubber.Scrub(input));
		}
	}
}
