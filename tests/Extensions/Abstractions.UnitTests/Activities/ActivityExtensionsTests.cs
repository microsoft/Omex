// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
		public void GetRootIdAsGuid_ReturnsProperGuid()
		{
			Activity.DefaultIdFormat = ActivityIdFormat.W3C;

			Activity activty = new Activity("RootIdTest");

			Guid? guidBeforeStart = activty.GetRootIdAsGuid();

			Assert.IsFalse(guidBeforeStart.HasValue);

			activty.Start();

			Guid? guidAfterStart = activty.GetRootIdAsGuid();

			Assert.IsTrue(guidAfterStart.HasValue);
			Assert.AreEqual(activty.RootId, guidAfterStart.GetValueOrDefault().ToString().Replace("-", ""));
		}

		[TestMethod]
		public void SetUserHash_SetsHash()
		{
			Activity activity1 = new Activity("UserHashTest1");
			Activity activity2 = new Activity("UserHashTest2");

			string initialUserHash = "intial hash value";
			string updatedUserHash = "updated hash value";

			activity1.SetUserHash(initialUserHash);
			activity1.SetUserHash(updatedUserHash);
			CheckThatKeyNotDuplicated(activity1.Baggage);

			Assert.AreEqual(updatedUserHash, activity1.GetUserHash());
			Assert.AreNotEqual(updatedUserHash, activity2.GetUserHash());
		}

		[TestMethod]
		public void MarkAsTransaction_AddsMarker()
		{
			Activity activity1 = new Activity("TransactionTest1");
			Activity activity2 = new Activity("TransactionTest2");

			activity1.MarkAsHealthCheck();
			activity1.MarkAsHealthCheck();
			CheckThatKeyNotDuplicated(activity1.Baggage);

			Assert.IsTrue(activity1.IsHealthCheck());
			Assert.IsFalse(activity2.IsHealthCheck());
		}

		[TestMethod]
		[Obsolete]
		public void SetObsoleteCorrelationId_SetsValue()
		{
			Activity activity1 = new Activity("CorrelationTest1");
			Activity activity2 = new Activity("CorrelationTest2");
			Guid initialCorrelation = Guid.NewGuid();
			Guid updatedCorrelation = Guid.NewGuid();

			activity1.SetObsoleteCorrelationId(initialCorrelation);
			activity1.SetObsoleteCorrelationId(updatedCorrelation);
			CheckThatKeyNotDuplicated(activity1.Baggage);

			Assert.AreEqual(updatedCorrelation, activity1.GetObsoleteCorrelationId());
			Assert.AreNotEqual(updatedCorrelation, activity2.GetObsoleteCorrelationId());
		}

		[TestMethod]
		[Obsolete]
		public void SetObsoleteTransactionId_SetsValue()
		{
			Activity activity1 = new Activity("TransactionIdTest1");
			Activity activity2 = new Activity("TransactionIdTest2");
			uint initialId = 117u;
			uint updatedId = 219u;

			activity1.SetObsoleteTransactionId(initialId);
			activity1.SetObsoleteTransactionId(updatedId);
			CheckThatKeyNotDuplicated(activity1.Baggage);

			Assert.AreEqual(updatedId, activity1.GetObsoleteTransactionId());
			Assert.AreNotEqual(updatedId, activity2.GetObsoleteTransactionId());
		}

		[DataTestMethod]
		[DataRow(TimedScopeResult.SystemError)]
		[DataRow(TimedScopeResult.ExpectedError)]
		[DataRow(TimedScopeResult.Success)]
		public void SetResult_SetsValue(TimedScopeResult result)
		{
			Activity activity1 = new Activity("SetResultTest1");
			Activity activity2 = new Activity("SetResultTest2");

			activity1.SetResult(TimedScopeResult.SystemError); // set some value intially to check that it could be updated
			activity1.SetResult(result);
			CheckThatKeyNotDuplicated(activity1.TagObjects);

			activity1.AssertResult(result);
			Assert.IsNull(activity2.GetTag(ActivityTagKeys.Result));
		}

		[TestMethod]
		public void MarkAsSuccess_SetSuccess()
		{
			Activity activity1 = new Activity("SetMarkAsSuccessTest1");
			
			activity1.SetResult(TimedScopeResult.SystemError); // set some value intially to check that it could be updated
			activity1.MarkAsSuccess();
			CheckThatKeyNotDuplicated(activity1.TagObjects);

			activity1.AssertResult(TimedScopeResult.Success);
		}

		[TestMethod]
		public void MarkAsSystemError_SetSystemError()
		{
			Activity activity1 = new Activity("SetMarkAsSystemErrorTest1");

			activity1.SetResult(TimedScopeResult.ExpectedError); // set some value intially to check that it could be updated
			activity1.MarkAsSystemError();
			CheckThatKeyNotDuplicated(activity1.TagObjects);

			activity1.AssertResult(TimedScopeResult.SystemError);
		}

		[TestMethod]
		public void MarkAsExpectedError_SetExpectedError()
		{
			Activity activity1 = new Activity("SetMarkAsExpectedErrorTest1");

			activity1.SetResult(TimedScopeResult.Success); // set some value intially to check that it could be updated
			activity1.MarkAsExpectedError();
			CheckThatKeyNotDuplicated(activity1.TagObjects);

			activity1.AssertResult(TimedScopeResult.ExpectedError);
		}

		[TestMethod]
		public void SetSubType_SetsValue()
		{
			Activity activity1 = new Activity("SetSubTypeTest1");
			Activity activity2 = new Activity("SetSubTypeTest2");
			string initialValue = "Initial sub type";
			string updatedValue = "Updated sub type";

			activity1.SetSubType(initialValue);
			activity1.SetSubType(updatedValue);
			CheckThatKeyNotDuplicated(activity1.TagObjects);

			Assert.AreEqual(updatedValue, activity1.GetTag(ActivityTagKeys.SubType));
			Assert.IsNull(activity2.GetTag(ActivityTagKeys.SubType));
		}

		[TestMethod]
		public void SetMetadata_SetsValue()
		{
			Activity activity1 = new Activity("SetMetadataTest1");
			Activity activity2 = new Activity("SetMetadataTest2");
			string initialValue = "initial metadata";
			string updatedValue = "Updated metadata";

			activity1.SetMetadata(initialValue);
			activity1.SetMetadata(updatedValue);
			CheckThatKeyNotDuplicated(activity1.TagObjects);

			Assert.AreEqual(updatedValue, activity1.GetTag(ActivityTagKeys.Metadata));
			Assert.IsNull(activity2.GetTag(ActivityTagKeys.Metadata));
		}

		private void CheckThatKeyNotDuplicated<T>(IEnumerable<KeyValuePair<string, T?>> collection)
			where T : class
		{
			string[] keys = collection.GroupBy(c => c.Key).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
			Assert.IsFalse(keys.Any(), "Duplicated keys found: " + string.Join(",", keys));
		}
	}
}
