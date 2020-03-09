// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	public class TimedScopeDefinitionsTests
	{
		[DataTestMethod]
		[DataRow(null)]
		[DataRow("")]
		public void CheckCreationWithWrongName(string activityName)
		{
			Assert.ThrowsException<ArgumentException>(() =>
			{
				new TimedScopeDefinition(activityName);
			});
		}


		[DataTestMethod]
		[DataRow("TestName")]
		public void CheckCreationCorrectName(string activityName)
		{
			TimedScopeDefinition definition = new TimedScopeDefinition(activityName);
			Assert.ReferenceEquals(activityName, definition.Name);
		}


		[TestMethod]
		public void TestEquiality()
		{
			string name = nameof(TestEquiality);

			TimedScopeDefinition definition1 = new TimedScopeDefinition(name);

			TimedScopeDefinition definition2 = new TimedScopeDefinition(name);

			Assert.IsTrue(definition1.Equals(definition2), "1 Equals 2");

			Assert.IsTrue(definition2.Equals(definition1), "2 Equals 1");

			Assert.IsTrue(definition1.Equals((object)definition2), "1 Equals (object)2");

			Assert.IsTrue(definition2.Equals((object)definition1), "2 Equals (object)1");

			Assert.IsTrue(definition1.GetHashCode() == definition2.GetHashCode(), "1 HashCode == 2 HashCode");

			Assert.IsTrue(definition1 == definition2, "1 == 2");

			Assert.IsTrue(definition2  == definition1, "2 == 1");

			Assert.IsFalse(definition1 != definition2, "1 != 2");

			Assert.IsFalse(definition2 != definition1, "2 != 1");
		}


		[TestMethod]
		public void TestNonEquiality()
		{
			TimedScopeDefinition definition1 = new TimedScopeDefinition("TestName1");

			TimedScopeDefinition definition2 = new TimedScopeDefinition("TestName2");

			Assert.IsFalse(definition1.Equals(definition2), "1 Equals 2");

			Assert.IsFalse(definition2.Equals(definition1), "2 Equals 1");

			Assert.IsFalse(definition1.Equals((object)definition2), "1 Equals (object)2");

			Assert.IsFalse(definition2.Equals((object)definition1), "2 Equals (object)1");

			Assert.IsFalse(definition1.GetHashCode() == definition2.GetHashCode(), "1 HashCode == 2 HashCode");

			Assert.IsFalse(definition1 == definition2, "1 == 2");

			Assert.IsFalse(definition2 == definition1, "2 == 1");

			Assert.IsTrue(definition1 != definition2, "1 != 2");

			Assert.IsTrue(definition2 != definition1, "2 != 1");
		}


		[TestMethod]
		public void TestEquialityWithEmpty()
		{
			TimedScopeDefinition definition = new TimedScopeDefinition(nameof(TestEquialityWithEmpty));

			Assert.IsFalse(definition.Equals(null), "definition Equals null");

			Assert.IsFalse(definition.Equals(default), "definition Equals default");

			Assert.IsFalse(definition == default, "definition == null");
		}
	}
}
