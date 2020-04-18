// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	[TestCategory(nameof(Activity))]
	public class ActivityExtensionsTests
	{
		[TestMethod]
		public void SetUserHash_SetsHash()
		{
			Activity activity1 = new Activity("UserHashTest1");
			Activity activity2 = new Activity("UserHashTest2");

			string userHash = "some hash value";

			activity1.SetUserHash(userHash);

			Assert.AreEqual(userHash, activity1.GetUserHash());
			Assert.AreNotEqual(userHash, activity2.GetUserHash());
		}

		[TestMethod]
		public void MarkAsTransaction_AddsMarker()
		{
			Activity activity1 = new Activity("TransactionTest1");
			Activity activity2 = new Activity("TransactionTest2");

			activity1.MarkAsTransaction();

			Assert.IsTrue(activity1.IsTransaction());
			Assert.IsFalse(activity2.IsTransaction());
		}

		[TestMethod]
		[Obsolete]
		public void SetObsoleteCorrelationId_SetsValue()
		{
			Activity activity1 = new Activity("CorrelationTest1");
			Activity activity2 = new Activity("CorrelationTest2");
			Guid correlation = Guid.NewGuid();

			activity1.SetObsoleteCorrelationId(correlation);

			Assert.AreEqual(correlation, activity1.GetObsoleteCorrelationId());
			Assert.AreNotEqual(correlation, activity2.GetObsoleteCorrelationId());
		}

		[TestMethod]
		[Obsolete]
		public void SetObsoleteTransactionId_SetsValue()
		{
			Activity activity1 = new Activity("TransactionIdTest1");
			Activity activity2 = new Activity("TransactionIdTest2");
			uint id = 117u;

			activity1.SetObsoleteTransactionId(id);

			Assert.AreEqual(id, activity1.GetObsolteteTransactionId());
			Assert.AreNotEqual(id, activity2.GetObsolteteTransactionId());
		}

		[DataTestMethod]
		[DataRow(TimedScopeResult.SystemError)]
		[DataRow(TimedScopeResult.ExpectedError)]
		[DataRow(TimedScopeResult.Success)]
		public void SetResult_SetsValue(TimedScopeResult result)
		{
			Activity activity1 = new Activity("SetResultTest1");
			Activity activity2 = new Activity("SetResultTest2");

			activity1.SetResult(result);

			activity1.AssertResult(result);
			Assert.IsNull(activity2.GetTag(ActivityTagKeys.Result));
		}

		[TestMethod]
		public void SetSubType_SetsValue()
		{
			Activity activity1 = new Activity("SetSubTypeTest1");
			Activity activity2 = new Activity("SetSubTypeTest2");
			string value = "Some sub type";

			activity1.SetSubType(value);

			Assert.AreEqual(value, activity1.GetTag(ActivityTagKeys.SubType));
			Assert.IsNull(activity2.GetTag(ActivityTagKeys.SubType));
		}

		[TestMethod]
		public void SetMetadata_SetsValue()
		{
			Activity activity1 = new Activity("SetMetadataTest1");
			Activity activity2 = new Activity("SetMetadataTest2");
			string value = "Some metadata";

			activity1.SetMetadata(value);

			Assert.AreEqual(value, activity1.GetTag(ActivityTagKeys.Metadata));
			Assert.IsNull(activity2.GetTag(ActivityTagKeys.Metadata));
		}
	}
}
