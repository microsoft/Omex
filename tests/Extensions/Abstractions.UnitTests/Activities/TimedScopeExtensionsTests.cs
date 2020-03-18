// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
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

			scope1.AssertResult(result);
			scope2.AssertResult(TimedScopeResult.SystemError);
		}


		[TestMethod]
		public void SetSubType_SetsValue()
		{
			TimedScope scope1 = CreateScope("SetSubTypeTest1");
			TimedScope scope2 = CreateScope("SetSubTypeTest2");
			string value = "Some sub type";

			scope1.SetSubType(value);

			Assert.AreEqual(value, scope1.GetTag(ActivityTagKeys.SubType));
			Assert.IsNull(scope2.GetTag(ActivityTagKeys.SubType));
		}


		[TestMethod]
		public void SetMetadata_SetsValue()
		{
			TimedScope scope1 = CreateScope("SetMetadataTest1");
			TimedScope scope2 = CreateScope("SetMetadataTest2");
			string value = "Some metadata";

			scope1.SetMetadata(value);

			Assert.AreEqual(value, scope1.GetTag(ActivityTagKeys.Metadata));
			Assert.IsNull(scope2.GetTag(ActivityTagKeys.Metadata));
		}


		private TimedScope CreateScope(string name) =>
			new TimedScope(new Activity(name), TimedScopeResult.SystemError);
	}
}
