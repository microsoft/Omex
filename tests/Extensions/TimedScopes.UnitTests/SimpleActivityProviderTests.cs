// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	public class SimpleActivityProviderTests
	{
		[DataTestMethod]
		[DataRow("TestName")]
		[DataRow("")]
		public void CheckActivityCreation(string expectedName)
		{
			Activity activity = new SimpleActivityProvider().Create(expectedName);

			Assert.IsNotNull(activity);

			string actualName = activity.OperationName;

			Assert.ReferenceEquals(expectedName, actualName);
		}
	}
}
