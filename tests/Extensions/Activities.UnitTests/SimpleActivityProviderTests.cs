// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	public class SimpleActivityProviderTests
	{
		[DataTestMethod]
		[DataRow("TestName")]
		[DataRow("123")]
		public void Create_CreatesActivity(string expectedName)
		{
			Activity activity = new SimpleActivityProvider().Create(new TimedScopeDefinition(expectedName));

			Assert.IsNotNull(activity);

			string actualName = activity.OperationName;

			Assert.AreEqual(expectedName, actualName);
		}
	}
}
