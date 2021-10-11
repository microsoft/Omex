// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.Omex.Extensions.Logging.Scrubbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests.Scrubbing
{
	[TestClass]
	[TestCategory("Shared")]
	public class LogScrubberUnitTests
	{
		[TestMethod]
		public void Constructor_ShouldNotScrub()
		{
			LogScrubber scrubberRules = new LogScrubber();
			Assert.IsFalse(scrubberRules.ShouldScrub, "No defined scrubber rules");
		}

		[TestMethod]
		public void Add_ShouldScrub()
		{
			LogScrubber scrubberRules = new LogScrubber();

			scrubberRules.Add(new ScrubberRule(new Regex("foobar*"), "redacted"));
			Assert.IsTrue(scrubberRules.ShouldScrub, "Should scrub based on rule");
		}


		[TestMethod]
		public void Scrub_WidthEmptyRules_ShouldNotScrub()
		{
			LogScrubber scrubberRules = new LogScrubber();
			string input = "foobar";
			Assert.AreEqual(input, scrubberRules.Scrub(input));
		}


		[TestMethod]
		public void Scrub_WithRule_ShouldScrub()
		{
			LogScrubber scrubberRules = new LogScrubber();
			scrubberRules.Add(new ScrubberRule(new Regex("foobar*"), "redacted"));

			Assert.AreEqual("hello redacted", scrubberRules.Scrub("hello foobarrr"));
		}


		[TestMethod]
		public void Scrub_WithMultipleRules_ShouldScrub()
		{
			LogScrubber scrubberRules = new LogScrubber();
			scrubberRules.Add(new ScrubberRule(new Regex("foobar*"), "redacted"));
			scrubberRules.Add(new ScrubberRule(new Regex("hello"), "goodbye"));

			Assert.AreEqual("goodbye redacted", scrubberRules.Scrub("hello foobarrr"));
		}


		[TestMethod]
		public void Scrub_WithoutInput_ShouldNotScrub()
		{
			LogScrubber scrubberRules = new LogScrubber();
			scrubberRules.Add(new ScrubberRule(new Regex("foobar*"), "redacted"));
			scrubberRules.Add(new ScrubberRule(new Regex("hello"), "goodbye"));

			Assert.AreEqual(string.Empty, scrubberRules.Scrub(string.Empty));
			Assert.AreEqual(null, scrubberRules.Scrub(null));
		}

	}
}
