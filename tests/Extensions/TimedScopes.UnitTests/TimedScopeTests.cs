using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Omex.Extensions.Abstractions.ReplayableLogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	public class TimedScopeTests
	{
		[TestMethod]
		public void ConstructorWithoutReplayer_WorksProperly()
		{
			CreateTimedScope(null);
		}


		[TestMethod]
		public void ConstructorWithReplayer_WorksProperly()
		{
			CreateTimedScope(new Mock<ILogEventReplayer>().Object);
		}


		[TestMethod]
		public void Start_StartsActivity()
		{
			(TimedScope scope, _) = CreateTimedScope(null);

			Assert.IsNull(scope.Activity.Id);

			scope.Start();

			Assert.IsNotNull(scope.Activity.Id);
		}


		[TestMethod]
		public void Stop_MultipleCallsIgnored()
		{
			(TimedScope scope, _) = CreateTimedScope(null);
			scope.Start();
			scope.Stop();

			Assert.IsTrue(scope.IsFinished);

			scope.Stop();
		}


		[TestMethod]
		public void Dispose_MultipleCallsIgnored()
		{
			(TimedScope scope, _) = CreateTimedScope(null);
			scope.Start();

			IDisposable disposable = scope;
			disposable.Dispose();

			Assert.IsTrue(scope.IsFinished);

			disposable.Dispose();
		}


		[TestMethod]
		public void Stop_CallsEventSource()
		{
			(TimedScope scope, Mock<ITimedScopeEventSender> source) = CreateTimedScope(null);

			scope.Start();

			scope.Stop();
			source.Verify(s => s.LogTimedScopeStopEvent(scope), Times.Once);
			source.Invocations.Clear();

			scope.Stop();
			source.Verify(s => s.LogTimedScopeStopEvent(It.IsAny<TimedScope>()), Times.Never);
		}


		[DataTestMethod]
		[DynamicData(nameof(AllResults), DynamicDataSourceType.Method)]
		public void Stop_NotCallsLogReplayerIfItNotExist(TimedScopeResult result)
		{
			Mock<ILogEventReplayer> replayer = new Mock<ILogEventReplayer>();
			(TimedScope scope, Mock<ITimedScopeEventSender> source) = CreateTimedScope(null);
			scope.Result = result;

			scope.Start();

			scope.Stop();
		}


		public static IEnumerable<object[]> AllResults() =>
			Enum.GetValues(typeof(TimedScopeResult)).Cast<object>().Select(e => new object[] { e });


		[DataTestMethod]
		[DataRow(TimedScopeResult.SystemError)]
		public void Stop_CallsLogReplayerInCaseOfError(TimedScopeResult result)
		{
			Mock<ILogEventReplayer> replayer = new Mock<ILogEventReplayer>();
			(TimedScope scope, Mock<ITimedScopeEventSender> source) = CreateTimedScope(replayer.Object);
			scope.Result = result;

			scope.Start();

			scope.Stop();
			replayer.Verify(r => r.ReplayLogs(scope.Activity), Times.Once);
			replayer.Invocations.Clear();

			scope.Stop();
			replayer.Verify(s => s.ReplayLogs(It.IsAny<Activity>()), Times.Never);
		}


		[DataTestMethod]
		[DataRow(TimedScopeResult.ExpectedError)]
		[DataRow(TimedScopeResult.Success)]
		public void Stop_NotCallsLogReplayerInCaseOfSucces(TimedScopeResult result)
		{
			Mock<ILogEventReplayer> replayer = new Mock<ILogEventReplayer>();
			(TimedScope scope, Mock<ITimedScopeEventSender> source) = CreateTimedScope(replayer.Object);
			scope.Result = result;

			scope.Start();

			scope.Stop();
			replayer.Verify(s => s.ReplayLogs(It.IsAny<Activity>()), Times.Never);
		}


		private (TimedScope, Mock<ITimedScopeEventSender>) CreateTimedScope(ILogEventReplayer? replayer)
		{
			Activity activity = new Activity("TestName");
			TimedScopeResult result = TimedScopeResult.Success;

			Mock<ITimedScopeEventSender> eventSource = new Mock<ITimedScopeEventSender>();

			TimedScope scope = new TimedScope(eventSource.Object, activity, result, replayer);

			Assert.ReferenceEquals(activity, scope.Activity);
			Assert.AreEqual(result, scope.Result);
			Assert.IsNotNull(scope.SubType);
			Assert.IsNotNull(scope.Metadata);
			Assert.IsFalse(scope.IsFinished);

			return (scope, eventSource);
		}
	}
}
