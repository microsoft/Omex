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
		public void Add_WithoutReplacementValue_ShouldNotThrow()
		{
			ScrubberRule scrubberRule = new(new Regex("pattern"), null);
			Assert.IsNotNull(scrubberRule, "Scrubber Rule got created");
		}
	}
}
