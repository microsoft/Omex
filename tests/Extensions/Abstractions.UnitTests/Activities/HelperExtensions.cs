// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	public static class HelperExtensions
	{
		public static void AssertResult(this Activity activity, TimedScopeResult expectedResult) =>
			Assert.AreEqual(ActivityResultStrings.ResultToString(expectedResult), activity.GetTag(ActivityTagKeys.Result));

		public static void AssertTag(this Activity activity, string tag, string expectedValue) =>
			Assert.AreEqual(expectedValue, activity.GetTag(tag));

		public static string? GetTag(this Activity activity, string tag) =>
			activity.Tags.FirstOrDefault(p => string.Equals(p.Key, tag, StringComparison.Ordinal)).Value;
	}
}
