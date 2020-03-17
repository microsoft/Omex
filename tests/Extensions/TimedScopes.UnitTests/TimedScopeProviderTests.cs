// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.ReplayableLogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	public class TimedScopeProviderTests
	{
		[TestMethod]
		public void CreateAndStart_ActivityCreatedWithReplay()
		{
			CreateAndValidateActivity("testNameWithReplay", new Mock<ILogEventReplayer>().Object);
		}


		[TestMethod]
		public void CreateAndStart_ActivityCreatedWithoutReplay()
		{
			CreateAndValidateActivity("testNameWithoutReplay", null);
		}


		private void CreateAndValidateActivity(string activityName, ILogEventReplayer? replayer)
		{
			TimedScopeResult result = TimedScopeResult.ExpectedError;

			Mock<ITimedScopeEventSender> eventSourceMock = new Mock<ITimedScopeEventSender>();
			Mock<IActivityProvider> activityProviderMock = new Mock<IActivityProvider>();
			Mock<Activity> activityMock = new Mock<Activity>(activityName);
			TimedScopeDefinition definition = new TimedScopeDefinition(activityName);

			activityProviderMock.Setup(p => p.Create(definition)).Returns(activityMock.Object);

			TimedScopeProvider provider = new TimedScopeProvider(
				eventSourceMock.Object,
				activityProviderMock.Object,
				replayer);

			TimedScope scope = provider.CreateAndStart(definition, result);

			Assert.AreEqual(result, scope.Result);
			Assert.ReferenceEquals(activityMock.Object, scope.Activity);
		}
	}
}
