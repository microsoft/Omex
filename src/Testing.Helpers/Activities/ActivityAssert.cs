// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Testing.Helpers
{
	/// <summary>
	/// Helper for getting information from Activity in unit tests
	/// </summary>
	public static class ActivityAssert
	{
		/// <summary>
		/// Assert that Activity has expected result value
		/// </summary>
		public static void AssertResult(this Activity activity, ActivityResult expectedResult) =>
			Assert.AreEqual(ActivityResultStrings.ResultToString(expectedResult), activity.GetTag(ActivityTagKeys.Result));

		/// <summary>
		/// Assert that Activity has expected tag value
		/// </summary>
		public static void AssertTag(this Activity activity, string tag, string expectedValue) =>
			Assert.AreEqual(expectedValue, activity.GetTag(tag));

		/// <summary>
		/// Get tag value from Activity
		/// </summary>
		/// <remarks>
		/// Done in inefficient way and should be used only in test scenarios
		/// </remarks>
		public static string? GetTag(this Activity activity, string tag) =>
			activity.Tags.FirstOrDefault(p => string.Equals(p.Key, tag, StringComparison.Ordinal)).Value;
	}
}
