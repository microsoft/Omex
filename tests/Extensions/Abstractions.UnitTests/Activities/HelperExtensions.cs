// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	internal static class HelperExtensions
	{
		public static void AssertResult(this TimedScope scope, TimedScopeResult expectedResult) =>
			Assert.AreEqual(ActivityResultStrings.ResultToString(expectedResult), scope.GetTag(ActivityTagKeys.Result));

		public static void AssertTag(this TimedScope scope, string tag, string expectedValue) =>
			Assert.AreEqual(expectedValue, scope.GetTag(tag));

		public static string GetTag(this TimedScope scope, string tag) =>
			scope.Activity.Tags.FirstOrDefault(p => string.Equals(p.Key, tag, StringComparison.Ordinal)).Value;
	}
}
