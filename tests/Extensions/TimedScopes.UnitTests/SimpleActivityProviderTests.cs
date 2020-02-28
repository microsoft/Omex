// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	public class SimpleActivityProviderTests
	{
		[DataTestMethod]
		[DataRow("TestName")]
		public void CheckActivityCreation(string expectedName)
		{
			Activity activity = new SimpleActivityProvider().Create(expectedName);

			Assert.IsNotNull(activity);

			string actualName = activity.OperationName;

			Assert.ReferenceEquals(expectedName, actualName);
		}


		[DataTestMethod]
		[DataRow(null)]
		[DataRow("")]
		public void CheckActivityParamitersValidation(string expectedName)
		{
			Assert.ThrowsException<ArgumentNullException>(() => new SimpleActivityProvider().Create(expectedName));
		}
	}
}
