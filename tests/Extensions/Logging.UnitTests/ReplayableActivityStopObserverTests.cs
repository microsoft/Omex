// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
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
		[DataRow(ActivityResult.SystemError, true, true)]
		[DataRow(ActivityResult.SystemError, false, false)]
		[DataRow(ActivityResult.ExpectedError, true, false)]
		public void OnStop_UsingReplaySettingsAndResult_CallesLogReplayIfNeeded(ActivityResult result, bool replayLog, bool shouldBeCalled)
		{
			Activity activity = new Activity(nameof(OnStop_UsingReplaySettingsAndResult_CallesLogReplayIfNeeded)).SetResult(result);

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

			public void AddReplayLog(Activity activity, LogMessageInformation logMessage) { }

			public bool IsReplayableMessage(LogLevel logLevel) => true;

			public void ReplayLogs(Activity activity) => Activity = activity;
		}
	}
}
