// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	[TestCategory(nameof(Activity))]
	public class TimedScopeExtensionsTests
	{
		[DataTestMethod]
		[DataRow(TimedScopeResult.SystemError)]
		[DataRow(TimedScopeResult.ExpectedError)]
		[DataRow(TimedScopeResult.Success)]
		public void SetResult_SetsValue(TimedScopeResult result)
		{
			TimedScope scope1 = CreateScope("SetResultTest1");
			TimedScope scope2 = CreateScope("SetResultTest2");

			scope1.SetResult(result);

			scope1.Activity.AssertResult(result);
			scope2.Activity.AssertResult(TimedScopeResult.SystemError);
		}

		[TestMethod]
		public void SetSubType_SetsValue()
		{
			TimedScope scope1 = CreateScope("SetSubTypeTest1");
			TimedScope scope2 = CreateScope("SetSubTypeTest2");
			string initialValue = "Initial sub type";
			string updatedValue = "Updated sub type";

			scope1.SetSubType(initialValue);
			scope1.SetSubType(updatedValue);

			Assert.AreEqual(updatedValue, scope1.Activity.GetTag(ActivityTagKeys.SubType));
			Assert.IsNull(scope2.Activity.GetTag(ActivityTagKeys.SubType));
		}

		[TestMethod]
		public void SetMetadata_SetsValue()
		{
			TimedScope scope1 = CreateScope("SetMetadataTest1");
			TimedScope scope2 = CreateScope("SetMetadataTest2");
			string initialValue = "initial metadata";
			string updatedValue = "Updated metadata";

			scope1.SetMetadata(initialValue);
			scope1.SetMetadata(updatedValue);

			Assert.AreEqual(updatedValue, scope1.Activity.GetTag(ActivityTagKeys.Metadata));
			Assert.IsNull(scope2.Activity.GetTag(ActivityTagKeys.Metadata));
		}

		[TestMethod]
		public void MarkAsHealthCheck_SetsValue()
		{
			TimedScope scope1 = CreateScope("MarkAsHealthCheckTest1").MarkAsHealthCheck();
			TimedScope scope2 = CreateScope("MarkAsHealthCheckTest2");

			Assert.IsTrue(scope1.Activity.IsHealthCheck());
			Assert.IsFalse(scope2.Activity.IsHealthCheck());
		}

		private TimedScope CreateScope(string name) =>
			new TimedScope(new Activity(name), TimedScopeResult.SystemError);
	}
}
