// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Logging.Internal.Replayable;
using Microsoft.Omex.Extensions.Logging.Replayable;
using Microsoft.Omex.Extensions.Logging.Scrubbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	public class OmexLoggerUnitTests
	{
		private static readonly Exception s_expectedPropagatedException = new("Test exception");

		[TestMethod]
		public void LogMessage_PropagatedToEventSource()
		{
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock();
			LogMessage(eventSourceMock, Array.Empty<ILogScrubbingRule>());
			eventSourceMock.Verify(s_logExpression, Times.Once);
		}

		[TestMethod]
		public void LogMessage_UseIsEnabledFlag()
		{
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock(false);
			LogMessage(eventSourceMock, Array.Empty<ILogScrubbingRule>());
			eventSourceMock.Verify(s_logExpression, Times.Never);
		}

		[TestMethod]
		public void LogMessage_UseIsReplayableMessageFlag()
		{
			const string suffix = nameof(LogMessage_UseIsReplayableMessageFlag);
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock();

			using Activity activity = CreateActivity(suffix);
			activity.Start();
			LogMessage(eventSourceMock, Array.Empty<ILogScrubbingRule>(), CreateLogReplayer(10));
			activity.Stop();

			eventSourceMock.Verify(s_logExpression, Times.Once);
			Assert.IsFalse(activity.GetReplayableLogs().Any(), "Log should not be stored for replay");
		}

		[TestMethod]
		public void LogMessage_ReplayedMessageAndExceptionSaved()
		{
			const string suffix = nameof(LogMessage_ReplayedMessageAndExceptionSaved);
			EventId eventId = CreateEventId(7, suffix);
			string message = GetLogMessage(suffix);
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock();

			using Activity activity = CreateActivity(suffix);
			activity.Start();
			(ILogger logger, _) = LogMessage(eventSourceMock, Array.Empty<ILogScrubbingRule>(), CreateLogReplayer(10), eventId.Id);
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
		public void Log_WithOneScrubber_ScrubbedReplayedAndExceptionSaved()
		{
			const string suffix = nameof(Log_WithOneScrubber_ScrubbedReplayedAndExceptionSaved);
			EventId eventId = CreateEventId(7, suffix);
			string message = GetLogMessage(suffix);
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock();

			using Activity activity = CreateActivity(suffix);
			activity.Start();
			(ILogger logger, _) = LogMessage(
				eventSourceMock,
				new List<ILogScrubbingRule> { new RegexLogScrubbingRule("Message", "[REDACTED]") },
				CreateLogReplayer(10),
				eventId.Id);
			logger.LogDebug(eventId, s_expectedPropagatedException, message);
			activity.Stop();

			eventSourceMock.Verify(s_logExpression, Times.Exactly(2));
			LogMessageInformation info = activity.GetReplayableLogs().Single();

			Assert.AreEqual(GetLogCategory(suffix), info.Category);
			Assert.AreEqual(eventId, info.EventId);
			StringAssert.Contains(info.Message, FormattableString.Invariant($"[REDACTED]-{suffix}"));
			StringAssert.Contains(info.Message, s_expectedPropagatedException.ToString());
		}

		[TestMethod]
		public void Log_WithMultipleScrubbers_ScrubbedReplayedAndExceptionSaved()
		{
			const string suffix = nameof(Log_WithMultipleScrubbers_ScrubbedReplayedAndExceptionSaved);
			EventId eventId = CreateEventId(7, suffix);
			string message = GetLogMessage(suffix);
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock();

			using Activity activity = CreateActivity(suffix);
			activity.Start();
			(ILogger logger, _) = LogMessage(
				eventSourceMock,
				new List<ILogScrubbingRule>
				{
					new RegexLogScrubbingRule("Mes", "[REDACTED]"),
					new RegexLogScrubbingRule("sage", "[REDACTED]")
				},
				CreateLogReplayer(10),
				eventId.Id);
			logger.LogDebug(eventId, s_expectedPropagatedException, message);
			activity.Stop();

			eventSourceMock.Verify(s_logExpression, Times.Exactly(2));
			LogMessageInformation info = activity.GetReplayableLogs().Single();

			Assert.AreEqual(GetLogCategory(suffix), info.Category);
			Assert.AreEqual(eventId, info.EventId);
			StringAssert.Contains(info.Message, FormattableString.Invariant($"[REDACTED][REDACTED]-{suffix}"));
			StringAssert.Contains(info.Message, s_expectedPropagatedException.ToString());
		}

		[TestMethod]
		public void LogMessage_ReplayedMessageSavedUntilTheLimit()
		{
			const string replayMessage1 = "ReplayMessage1";
			Exception exception1 = new ArgumentException("Error");
			const string replayMessage2 = "ReplayMessage2";
			Exception exception2 = new NullReferenceException("Error");

			const string suffix = nameof(LogMessage_ReplayedMessageSavedUntilTheLimit);
			const int eventId = 7;
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock();

			using Activity activity = CreateActivity(suffix);
			activity.Start();
			(ILogger logger, _) = LogMessage(eventSourceMock, Array.Empty<ILogScrubbingRule>(), CreateLogReplayer(2), eventId);
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
			(ILogger logger, Mock<IExternalScopeProvider> scopeProviderMock) = LogMessage(CreateEventSourceMock(), Array.Empty<ILogScrubbingRule>());

			object obj = new();
			using IDisposable resultMock = new Mock<IDisposable>().Object;
			scopeProviderMock.Setup(p => p.Push(obj)).Returns(resultMock);

			using IDisposable result = logger.BeginScope(obj);

			scopeProviderMock.Verify(p => p.Push(obj), Times.Once);
			Assert.AreEqual(resultMock, result);
		}

		private static Mock<ILogEventSender> CreateEventSourceMock(bool isEnabled = true)
		{
			Mock<ILogEventSender> eventSourceMock = new();
			eventSourceMock.Setup(e => e.IsEnabled(It.IsAny<LogLevel>())).Returns(isEnabled);
			return eventSourceMock;
		}

		private static (ILogger, Mock<IExternalScopeProvider>) LogMessage(
			IMock<ILogEventSender> eventSourceMock,
			IEnumerable<ILogScrubbingRule> textScrubbers,
			ILogEventReplayer? logEventReplayer = null,
			int eventId = 0,
			[CallerMemberName] string suffix = "")
		{
			Mock<IExternalScopeProvider> scopeProviderMock = new();
			ILogger logger = new OmexLogger(eventSourceMock.Object, scopeProviderMock.Object, textScrubbers, GetLogCategory(suffix), logEventReplayer);

			logger.LogError(CreateEventId(eventId, suffix), s_expectedPropagatedException, GetLogMessage(suffix));

			return (logger, scopeProviderMock);
		}

		private static ILogEventReplayer CreateLogReplayer(uint replayLimit)
		{
			Mock<IOptionsMonitor<OmexLoggingOptions>> mock = new();
			mock.Setup(m => m.CurrentValue).Returns(new OmexLoggingOptions
			{
				ReplayLogsInCaseOfError = true,
				MaxReplayedEventsPerActivity = replayLimit
			});

			return new OmexLogEventReplayer(new Mock<ILogEventSender>().Object, mock.Object);
		}

		private static string GetLogMessage(string suffix) =>
			FormattableString.Invariant($"Message-{suffix}");

		private static string GetLogCategory(string suffix) =>
			FormattableString.Invariant($"Category-{suffix}");

		private static EventId CreateEventId(int id, string suffix) =>
			new(id, FormattableString.Invariant($"EventId-{suffix}"));

		private static Activity CreateActivity(string suffix) =>
			new(FormattableString.Invariant($"Activity-{suffix}"));

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
