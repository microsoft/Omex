// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.Omex.Extensions.Logging.Replayable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hosting.Services.UnitTests
{
	[TestClass]
	public class ReplayableActivityStopObserverTests
	{
		[DataTestMethod]
		[DataRow(TimedScopeResult.SystemError, true, true)]
		[DataRow(TimedScopeResult.SystemError, false, false)]
		[DataRow(TimedScopeResult.ExpectedError, true, false)]
		public void OnStop_UsingReplaySettingsAndResult_CallesLogReplayIfNeeded(TimedScopeResult result, bool replayLog, bool shouldBeCalled)
		{
			Activity activity = new Activity(nameof(OnStop_UsingReplaySettingsAndResult_CallesLogReplayIfNeeded));
			TimedScope timedScope = new TimedScope(activity, result);
			IOptions<OmexLoggingOptions> options = Options.Create(new OmexLoggingOptions { ReplayLogsInCaseOfError = replayLog });
			LogEventReplayerMock replayerMock = new LogEventReplayerMock();
			ReplayableActivityStopObserver observer = new ReplayableActivityStopObserver(replayerMock, options);
			observer.OnStop(activity, null);

			if (shouldBeCalled)
			{
				Assert.AreEqual(activity, replayerMock.Activity);
			}
			else
			{
				Assert.IsNull(replayerMock.Activity);
			}
		}

		private class LogEventReplayerMock : ILogEventReplayer
		{
			public Activity? Activity { get; private set; }

			public void ReplayLogs(Activity activity) => Activity = activity;
		}
	}
}
