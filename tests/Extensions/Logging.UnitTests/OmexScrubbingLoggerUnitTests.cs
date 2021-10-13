// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Logging.Internal.EventSource;
using Microsoft.Omex.Extensions.Logging.Internal.Replayable;
using Microsoft.Omex.Extensions.Logging.Replayable;
using Microsoft.Omex.Extensions.Logging.Scrubbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	public class OmexScrubbingLoggerUnitTests
	{
		private static readonly Exception s_expectedPropagatedException = new Exception("Test exception");

		[TestInitialize]
		public void Initialize()
		{
			LogScrubber.Instance.ClearRules();
		}

		[TestMethod]
		public void LogMessage_PropagatedToEventSource()
		{
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock(isEnabled: true);
			LogMessage(eventSourceMock);
			eventSourceMock.Verify(s_logExpression, Times.Once);
		}

		[TestMethod]
		public void LogMessage_UseIsEnabledFlag()
		{
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock(isEnabled: false);
			LogMessage(eventSourceMock);
			eventSourceMock.Verify(s_logExpression, Times.Never);
		}

		[TestMethod]
		public void LogMessage_UseIsReplayableMessageFlag()
		{
			string suffix = nameof(LogMessage_UseIsReplayableMessageFlag);
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock();

			Activity activity = CreateActivity(suffix);
			activity.Start();
			LogMessage(eventSourceMock, CreateLogReplayer(10));
			activity.Stop();

			eventSourceMock.Verify(s_logExpression, Times.Once);
			Assert.IsFalse(activity.GetReplayableLogs().Any(), "Log should not be stored for replay");
		}

		[TestMethod]
		public void LogMessage_ReplayedMessageAndExceptionSaved()
		{
			string suffix = nameof(LogMessage_ReplayedMessageAndExceptionSaved);
			EventId eventId = CreateEventId(7, suffix);
			string message = GetLogMessage(suffix);
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock();

			Activity activity = CreateActivity(suffix);
			activity.Start();
			(ILogger logger, _) = LogMessage(eventSourceMock, logEventReplayer: CreateLogReplayer(10), eventId.Id);
			logger.LogDebug(eventId, s_expectedPropagatedException, message);
			activity.Stop();

			eventSourceMock.Verify(s_logExpression, Times.Exactly(2));
			LogMessageInformation info = activity.GetReplayableLogs().Single();

			Assert.AreEqual(GetLogCategory(suffix), info.Category);
			Assert.AreEqual(eventId, info.EventId);
			StringAssert.Contains(info.Message, message);
			StringAssert.Contains(info.Message, s_expectedPropagatedException.ToString());
		}

		[TestMethod]
		public void LogMessage_ReplayedMessageSavedUnitilTheLimit()
		{
			string replayMessage1 = "ReplayMessage1";
			Exception exception1 = new ArgumentException("Error");
			string replayMessage2 = "ReplayMessage2";
			Exception exception2 = new NullReferenceException("Error");

			string suffix = nameof(LogMessage_ReplayedMessageSavedUnitilTheLimit);
			int eventId = 7;
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock();

			Activity activity = CreateActivity(suffix);
			activity.Start();
			(ILogger logger, _) = LogMessage(eventSourceMock, logEventReplayer: CreateLogReplayer(2), eventId);
			logger.LogDebug(new DivideByZeroException(), "LostMessage"); // would be lost due overflow
			logger.LogDebug(exception1, replayMessage1);
			logger.LogDebug(exception2, replayMessage2);
			activity.Stop();

			eventSourceMock.Verify(s_logExpression, Times.Exactly(4));
			List<LogMessageInformation> info = activity.GetReplayableLogs().ToList();

			Assert.AreEqual(2, info.Count);
			StringAssert.Contains(info[0].Message, replayMessage1);
			StringAssert.Contains(info[0].Message, exception1.ToString());
			StringAssert.Contains(info[1].Message, replayMessage2);
			StringAssert.Contains(info[1].Message, exception2.ToString());
		}

		[TestMethod]
		public void BeginScope_ScopeProperlyCreated()
		{
			(ILogger logger, Mock<IExternalScopeProvider> scopeProvicedMock) = LogMessage(CreateEventSourceMock());

			object obj = new object();
			IDisposable resultMock = new Mock<IDisposable>().Object;
			scopeProvicedMock.Setup(p => p.Push(obj)).Returns(resultMock);

			IDisposable result = logger.BeginScope(obj);

			scopeProvicedMock.Verify(p => p.Push(obj), Times.Once);
			Assert.AreEqual(resultMock, result);
		}

		[TestMethod]
		public void Log_ShouldScrub()
		{
			LogScrubber.Instance.AddRule(new ScrubberRule(new Regex("Replay"), "redacted"));
			LogScrubber.Instance.AddRule(new ScrubberRule(new Regex("Error"), "redacted"));

			string replayMessage1 = "ReplayMessage1";
			Exception exception1 = new ArgumentException("Error");
			string replayMessage2 = "ReplayMessage2";
			Exception exception2 = new NullReferenceException("Error");

			string suffix = nameof(LogMessage_ReplayedMessageSavedUnitilTheLimit);
			int eventId = 7;
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock();

			Activity activity = CreateActivity(suffix);
			activity.Start();
			(ILogger logger, _) = LogMessage(eventSourceMock, logEventReplayer: CreateLogReplayer(2), eventId);
			logger.LogDebug(new DivideByZeroException(), "LostMessage"); // would be lost due overflow
			logger.LogDebug(exception1, replayMessage1);
			logger.LogDebug(exception2, replayMessage2);
			activity.Stop();

			eventSourceMock.Verify(s_logExpression, Times.Exactly(4));
			List<LogMessageInformation> info = activity.GetReplayableLogs().ToList();

			Assert.AreEqual(2, info.Count);
			StringAssert.Contains(info[0].Message, "redactedMessage1");
			StringAssert.Contains(info[0].Message, "redacted");
			StringAssert.Contains(info[1].Message, "redactedMessage2");
			StringAssert.Contains(info[1].Message, "redacted");
		}

		private static Mock<ILogEventSender> CreateEventSourceMock(bool isEnabled = true)
		{
			Mock<ILogEventSender> eventSourceMock = new Mock<ILogEventSender>();
			eventSourceMock.Setup(e => e.IsEnabled(It.IsAny<LogLevel>())).Returns(isEnabled);
			return eventSourceMock;
		}

		private static (ILogger, Mock<IExternalScopeProvider>) LogMessage(Mock<ILogEventSender> eventSourceMock, ILogEventReplayer? logEventReplayer = null, int eventId = 0, [CallerMemberName]string suffix = "")
		{
			Mock<IExternalScopeProvider> scopeProvicedMock = new Mock<IExternalScopeProvider>();
			ILogger logger = new OmexScrubbingLogger(eventSourceMock.Object, scopeProvicedMock.Object, GetLogCategory(suffix), logEventReplayer);

			logger.LogError(CreateEventId(eventId, suffix), s_expectedPropagatedException, GetLogMessage(suffix));

			return (logger, scopeProvicedMock);
		}

		private static ILogEventReplayer CreateLogReplayer(uint replayLimit)
		{
			Mock<IOptionsMonitor<OmexLoggingOptions>> mock = new Mock<IOptionsMonitor<OmexLoggingOptions>>();
			mock.Setup(m => m.CurrentValue).Returns(new OmexLoggingOptions()
			{
				ReplayLogsInCaseOfError = true,
				MaxReplayedEventsPerActivity = replayLimit
			});

			return new OmexLogEventReplayer(new Mock<ILogEventSender>().Object, mock.Object);
		}

		private static string GetLogMessage(string suffix) => FormattableString.Invariant($"Message-{suffix}");

		private static string GetLogCategory(string suffix) => FormattableString.Invariant($"Category-{suffix}");

		private static EventId CreateEventId(int id, string suffix) => new EventId(id, FormattableString.Invariant($"EventId-{suffix}"));

		private static Activity CreateActivity(string suffix) => new Activity(FormattableString.Invariant($"Activity-{suffix}"));

		private static readonly Expression<Action<ILogEventSender>> s_logExpression = e =>
			e.LogMessage(
				It.IsAny<Activity>(),
				It.IsAny<string>(),
				It.IsAny<LogLevel>(),
				It.IsAny<EventId>(),
				It.IsAny<int>(),
				It.IsAny<string>(),
				It.IsAny<Exception>());
	}
}
