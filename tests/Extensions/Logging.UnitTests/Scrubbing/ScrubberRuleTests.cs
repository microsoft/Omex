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
	}
}
