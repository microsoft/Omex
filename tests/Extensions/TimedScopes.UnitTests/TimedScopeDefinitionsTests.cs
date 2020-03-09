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
	}
}
