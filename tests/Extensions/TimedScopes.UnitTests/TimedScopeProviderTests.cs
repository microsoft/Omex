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
			CreateAndValidateActivity(new Mock<ILogReplayer>().Object);
		}


		[TestMethod]
		public void CheckThatTimedScopeProvicerCreatesActivityWithoutReplayer()
		{
			CreateAndValidateActivity(null);
		}


		private void CreateAndValidateActivity(ILogReplayer? replayer)
		{
			string activityName = "testName";
			TimedScopeResult result = TimedScopeResult.ExpectedError;

			Mock<ITimedScopeEventSource> eventSourceMock = new Mock<ITimedScopeEventSource>();
			Mock<IActivityProvider> activityProviderMock = new Mock<IActivityProvider>();
			Mock<Activity> activityMock = new Mock<Activity>(activityName);
			activityProviderMock.Setup(p => p.Create(activityName)).Returns(activityMock.Object);

			TimedScopeProvider provider = new TimedScopeProvider(eventSourceMock.Object, activityProviderMock.Object, replayer);

			TimedScope scope = provider.Start(activityName, result);

			Assert.AreEqual(result, scope.Result);
			Assert.ReferenceEquals(activityMock.Object, scope.Activity);
		}
	}
}
