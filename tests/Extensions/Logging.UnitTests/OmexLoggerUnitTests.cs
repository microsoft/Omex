// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Logging.Replayable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	public class OmexLoggerUnitTests
	{
		private static readonly Exception s_expectedPropagatedException = new Exception("Test exception");

		[TestMethod]
		public void LogMessage_PropagatedToEventSource()
		{
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock(isEnabled: true);
			LogMessage(eventSourceMock);
			eventSourceMock.Verify(m_logExpression, Times.Once);
		}

		[TestMethod]
		public void LogMessage_UseIsEnabledFlag()
		{
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock(isEnabled: false);
			LogMessage(eventSourceMock);
			eventSourceMock.Verify(m_logExpression, Times.Never);
		}

		[TestMethod]
		public void LogMessage_UseIsReplayableMessageFlag()
		{
			string suffix = nameof(LogMessage_UseIsReplayableMessageFlag);
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock(isReplayable: false);

			ReplayableActivity activity = CreateActivity(suffix);
			activity.Start();
			LogMessage(eventSourceMock);
			activity.Stop();

			eventSourceMock.Verify(m_logExpression, Times.Once);
			Assert.IsFalse(activity.GetLogEvents().Any(), "Log should not be stored for replay");
		}

		[TestMethod]
		public void LogMessage_ReplayedMessageSaved()
		{
			string suffix = nameof(LogMessage_ReplayedMessageSaved);
			int eventId = 7;
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock(isReplayable: true);

			ReplayableActivity activity = CreateActivity(suffix);
			activity.Start();
			LogMessage(eventSourceMock, eventId);
			activity.Stop();

			eventSourceMock.Verify(m_logExpression, Times.Once);
			LogMessageInformation info = activity.GetLogEvents().Single();

			Assert.AreEqual(GetLogCategory(suffix), info.Category);
			Assert.AreEqual(GetLogMessage(suffix), info.Message);
			Assert.AreEqual(CreateEventId(eventId, suffix), info.EventId);
		}

		[TestMethod]
		public void LogMessage_ReplayedMessageSavedUnitilTheLimit()
		{
			string replayMessage1 = "ReplayMessage1";
			string replayMessage2 = "ReplayMessage2";

			string suffix = nameof(LogMessage_ReplayedMessageSaved);
			int eventId = 7;
			Mock<ILogEventSender> eventSourceMock = CreateEventSourceMock(isReplayable: true);

			ReplayableActivity activity = CreateActivity(suffix);
			activity.Start();
			(ILogger logger, _) = LogMessage(eventSourceMock, eventId);
			logger.LogDebug(s_expectedPropagatedException, replayMessage1);
			logger.LogDebug(s_expectedPropagatedException, replayMessage2);
			activity.Stop();

			eventSourceMock.Verify(m_logExpression, Times.Exactly(3));
			List<LogMessageInformation> info = activity.GetLogEvents().ToList();

			Assert.AreEqual(2, info.Count);
			Assert.AreEqual(replayMessage1, info[0].Message);
			Assert.AreEqual(replayMessage2, info[1].Message);
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

		private Mock<ILogEventSender> CreateEventSourceMock(bool isEnabled = true, bool isReplayable = true)
		{
			Mock<ILogEventSender> eventSourceMock = new Mock<ILogEventSender>();
			eventSourceMock.Setup(e => e.IsEnabled(It.IsAny<LogLevel>())).Returns(isEnabled);
			eventSourceMock.Setup(e => e.IsReplayableMessage(It.IsAny<LogLevel>())).Returns(isReplayable);
			return eventSourceMock;
		}

		private (ILogger, Mock<IExternalScopeProvider>) LogMessage(Mock<ILogEventSender> eventSourceMock, int eventId = 0, [CallerMemberName]string suffix = "")
		{
			Mock<IExternalScopeProvider> scopeProvicedMock = new Mock<IExternalScopeProvider>();
			ILogger logger = new OmexLogger(eventSourceMock.Object, scopeProvicedMock.Object, GetLogCategory(suffix));

			logger.LogError(CreateEventId(eventId, suffix), s_expectedPropagatedException, GetLogMessage(suffix));

			return (logger, scopeProvicedMock);
		}

		private string GetLogMessage(string suffix) => FormattableString.Invariant($"Message-{suffix}");

		private string GetLogCategory(string suffix) => FormattableString.Invariant($"Category-{suffix}");

		private EventId CreateEventId(int id, string suffix) => new EventId(id, FormattableString.Invariant($"EventId-{suffix}"));

		private ReplayableActivity CreateActivity(string suffix) => new ReplayableActivity(FormattableString.Invariant($"Activity-{suffix}"), 2);

		private readonly Expression<Action<ILogEventSender>> m_logExpression = e =>
			e.LogMessage(
				It.IsAny<Activity>(),
				It.IsAny<string>(),
				It.IsAny<LogLevel>(),
				It.IsAny<EventId>(),
				It.IsAny<int>(),
				It.IsAny<string>(),
				s_expectedPropagatedException);
	}
}
