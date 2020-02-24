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
		public void CheckThatTimedScopeProvicerCreatesProperActivity()
		{
			string activityName = "testName";
			TimedScopeResult result = TimedScopeResult.SystemError;

			Mock<ITimedScopeEventSource> eventSourceMock = new Mock<ITimedScopeEventSource>();
			Mock<IActivityProvider> activityProviderMock = new Mock<IActivityProvider>();
			Mock<Activity> activityMock = new Mock<Activity>(activityName);
			activityProviderMock.Setup(p => p.Create(activityName)).Returns(activityMock.Object);
			Mock<ILogReplayer> replayerMock = new Mock<ILogReplayer>();

			TimedScopeProvider provider = new TimedScopeProvider(eventSourceMock.Object, activityProviderMock.Object, replayerMock.Object);

			TimedScope scope = provider.Start(activityName, result);

			Assert.ReferenceEquals(activityMock.Object, scope.Activity);
		}
	}
}
