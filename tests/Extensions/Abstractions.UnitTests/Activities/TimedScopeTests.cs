// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	[TestCategory(nameof(Activity))]
	public class TimedScopeTests
	{
		[TestMethod]
		public void Constructor_WorksProperly()
		{
			CreateTimedScope();
		}

		[TestMethod]
		public void Start_StartsActivity()
		{
			TimedScope scope = CreateTimedScope();

			Assert.IsNull(scope.Activity.Id);

			scope.Start();

			Assert.IsNotNull(scope.Activity.Id);
		}

		[TestMethod]
		public void Stop_WhenCalledMultipleTimes_IgnoresSuperfluousCalls()
		{
			TimedScope scope = CreateTimedScope();
			scope.Start();
			scope.Stop();
			scope.Stop();
		}

		[TestMethod]
		public void Dispose_WhenCalledMultipleTimes_IgnoresSuperfluousCalls()
		{
			TimedScope scope = CreateTimedScope();
			scope.Start();

			IDisposable disposable = scope;
			disposable.Dispose();
			disposable.Dispose();
		}

		private TimedScope CreateTimedScope()
		{
			Activity activity = new Activity("TestName");
			TimedScopeResult result = TimedScopeResult.Success;

			TimedScope scope = new TimedScope(activity, result);

			Assert.AreEqual(activity, scope.Activity);
			scope.Activity.AssertResult(result);

			return scope;
		}
	}
}
