// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	public class TimedScopeProviderTests
	{
		[TestMethod]
		public void CheckThatTimedScopeProvicerCreatesActivity()
		{
			CreateAndValidateActivity("testNameWithReplay", new Mock<ILogEventReplayer>().Object);
		}


		[TestMethod]
		public void CheckThatTimedScopeProvicerCreatesActivityWithoutReplayer()
		{
			CreateAndValidateActivity("testNameWithoutReplay", null);
		}


		private void CreateAndValidateActivity(string activityName, ILogEventReplayer? replayer)
		{
			TimedScopeResult result = TimedScopeResult.ExpectedError;

			Mock<ITimedScopeEventSender> eventSourceMock = new Mock<ITimedScopeEventSender>();
			Mock<IActivityProvider> activityProviderMock = new Mock<IActivityProvider>();
			Mock<Activity> activityMock = new Mock<Activity>(activityName);
			activityProviderMock.Setup(p => p.Create(activityName)).Returns(activityMock.Object);

			TimedScopeProvider provider = new TimedScopeProvider(
				eventSourceMock.Object,
				activityProviderMock.Object,
				replayer);

			TimedScope scope = provider.Start(activityName, result);

			Assert.AreEqual(result, scope.Result);
			Assert.ReferenceEquals(activityMock.Object, scope.Activity);
		}


		[DataTestMethod]
		[DataRow(null)]
		[DataRow("")]
		public void CheckCreationWithWrongName(string activityName)
		{
			TimedScopeResult result = TimedScopeResult.ExpectedError;

			Mock<ITimedScopeEventSender> eventSourceMock = new Mock<ITimedScopeEventSender>();
			Mock<IActivityProvider> activityProviderMock = new Mock<IActivityProvider>();
			Mock<Activity> activityMock = new Mock<Activity>(activityName);

			activityProviderMock
				.Setup(p => p.Create(It.Is<string>(n => !string.IsNullOrEmpty(n))))
				.Returns(activityMock.Object);

			TimedScopeProvider provider = new TimedScopeProvider(
				eventSourceMock.Object,
				activityProviderMock.Object,
				null);

			Assert.ThrowsException<ArgumentException>(() => provider.Start(activityName, result));
		}
	}
}
