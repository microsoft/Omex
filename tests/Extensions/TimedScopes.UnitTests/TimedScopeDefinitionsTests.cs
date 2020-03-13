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
		public void Constructor_ThrowsException(string activityName)
		{
			Assert.ThrowsException<ArgumentException>(() =>
			{
				new TimedScopeDefinition(activityName);
			});
		}


		[DataTestMethod]
		[DataRow("TestName")]
		public void Constructor_PropagatesName(string activityName)
		{
			TimedScopeDefinition definition = new TimedScopeDefinition(activityName);
			Assert.ReferenceEquals(activityName, definition.Name);
		}


		[TestMethod]
		public void Equals_WorksProperly()
		{
			string name = nameof(Equals_WorksProperly);

			TimedScopeDefinition definition1 = new TimedScopeDefinition(name);

			TimedScopeDefinition definition2 = new TimedScopeDefinition(name);

			Assert.IsTrue(definition1.Equals(definition2), "1 Equals 2");

			Assert.IsTrue(definition2.Equals(definition1), "2 Equals 1");

			Assert.IsTrue(definition1.Equals((object)definition2), "1 Equals (object)2");

			Assert.IsTrue(definition2.Equals((object)definition1), "2 Equals (object)1");

			Assert.AreEqual(definition1.GetHashCode(), definition2.GetHashCode(), "1 HashCode equals 2 HashCode");

			Assert.IsTrue(definition1 == definition2, "1 == 2");

			Assert.IsTrue(definition2 == definition1, "2 == 1");

			Assert.IsFalse(definition1 != definition2, "1 != 2");

			Assert.IsFalse(definition2 != definition1, "2 != 1");
		}


		[TestMethod]
		public void NotEquals_WorksProperly()
		{
			TimedScopeDefinition definition1 = new TimedScopeDefinition("TestName1");

			TimedScopeDefinition definition2 = new TimedScopeDefinition("TestName2");

			Assert.IsFalse(definition1.Equals(definition2), "1 Equals 2");

			Assert.IsFalse(definition2.Equals(definition1), "2 Equals 1");

			Assert.IsFalse(definition1.Equals((object)definition2), "1 Equals (object)2");

			Assert.IsFalse(definition2.Equals((object)definition1), "2 Equals (object)1");

			Assert.AreNotEqual(definition1.GetHashCode(), definition2.GetHashCode(), "1 HashCode NotEquals 2 HashCode");

			Assert.IsFalse(definition1 == definition2, "1 == 2");

			Assert.IsFalse(definition2 == definition1, "2 == 1");

			Assert.IsTrue(definition1 != definition2, "1 != 2");

			Assert.IsTrue(definition2 != definition1, "2 != 1");
		}


		[TestMethod]
		public void EquilsWithEmpty_WorksProperly()
		{
			TimedScopeDefinition definition = new TimedScopeDefinition(nameof(EquilsWithEmpty_WorksProperly));

			Assert.IsFalse(definition.Equals(null), "definition Equals null");

			Assert.IsFalse(definition.Equals(default), "definition Equals default");

			Assert.IsFalse(definition == default, "definition == null");
		}
	}
}
