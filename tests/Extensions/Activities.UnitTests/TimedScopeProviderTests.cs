// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	public class TimedScopeProviderTests
	{
		[TestMethod]
		public void CreateAndStart_ActivityCreated()
		{
			CreateAndValidateActivity("testName");
		}

		private void CreateAndValidateActivity(string activityName)
		{
			ActivityResult result = ActivityResult.ExpectedError;

			Mock<IActivityProvider> activityProviderMock = new Mock<IActivityProvider>();
			Mock<Activity> activityMock = new Mock<Activity>(activityName);
			TimedScopeDefinition definition = new TimedScopeDefinition(activityName);

			activityProviderMock.Setup(p => p.Create(definition)).Returns(activityMock.Object);

			TimedScopeProvider provider = new TimedScopeProvider(activityProviderMock.Object);

			TimedScope scope = provider.CreateAndStart(definition, result);

			Assert.IsNotNull(scope);
		}
	}
}
