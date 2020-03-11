// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
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
	}
}
