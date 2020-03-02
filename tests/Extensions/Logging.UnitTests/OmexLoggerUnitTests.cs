// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Logging.Replayable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	public class OmexLoggerUnitTests
	{
		[TestMethod]
		public void MessagePropagatedToEventSource()
		{
			Mock<ILogsEventSource> eventSourceMock = CreateEventSourceMock(isEnabled: true);
			LogMessage(nameof(MessagePropagatedToEventSource), eventSourceMock);
			eventSourceMock.Verify(m_logExpression, Times.Once);
		}


		[TestMethod]
		public void IsEnabledUsed()
		{
			Mock<ILogsEventSource> eventSourceMock = CreateEventSourceMock(isEnabled: false);
			LogMessage(nameof(IsEnabledUsed), eventSourceMock);
			eventSourceMock.Verify(m_logExpression, Times.Never);
		}


		[TestMethod]
		public void IsReplayableMessageUsed()
		{
			Mock<ILogsEventSource> eventSourceMock = CreateEventSourceMock(isReplayable: false);

			ReplayableActivity activity = new ReplayableActivity(nameof(IsReplayableMessageUsed));
			activity.Start();
			LogMessage(nameof(IsReplayableMessageUsed), eventSourceMock);
			activity.Stop();

			eventSourceMock.Verify(m_logExpression, Times.Once);
			Assert.AreEqual(0, activity.GetLogEvents().Count(), "Log should not be stored for replay");
		}


		[TestMethod]
		public void ReplayedMessageSaved()
		{
			Mock<ILogsEventSource> eventSourceMock = CreateEventSourceMock(isReplayable: true);

			ReplayableActivity activity = new ReplayableActivity(nameof(ReplayedMessageSaved));
			activity.Start();
			LogMessage(nameof(ReplayedMessageSaved), eventSourceMock);
			activity.Stop();

			eventSourceMock.Verify(m_logExpression, Times.Once);
			Assert.AreEqual(1, activity.GetLogEvents().Count(), "Log should be stored for replay");
		}


		[TestMethod]
		public void ScopeTest()
		{
			(ILogger logger, Mock<IExternalScopeProvider> scopeProvicedMock) = LogMessage(nameof(ReplayedMessageSaved), CreateEventSourceMock());

			object obj = new object();
			IDisposable resultMock = new Mock<IDisposable>().Object;
			scopeProvicedMock.Setup(p => p.Push(obj)).Returns(resultMock);

			IDisposable result = logger.BeginScope(obj);

			scopeProvicedMock.Verify(p => p.Push(obj), Times.Once);
			Assert.AreEqual(resultMock, result);
		}


		private Mock<ILogsEventSource> CreateEventSourceMock(bool isEnabled = true, bool isReplayable = true)
		{
			Mock<ILogsEventSource> eventSourceMock = new Mock<ILogsEventSource>();
			eventSourceMock.Setup(e => e.IsEnabled(It.IsAny<LogLevel>())).Returns(isEnabled);
			eventSourceMock.Setup(e => e.IsReplayableMessage(It.IsAny<LogLevel>())).Returns(isReplayable);
			return eventSourceMock;
		}


		private (ILogger, Mock<IExternalScopeProvider>) LogMessage(string suffix, Mock<ILogsEventSource> eventSourceMock)
		{
			Mock<IExternalScopeProvider> scopeProvicedMock = new Mock<IExternalScopeProvider>();
			ILogger logger = new OmexLogger(eventSourceMock.Object, scopeProvicedMock.Object, $"Category-{suffix}");
			logger.LogError($"Message-{suffix}");
			return (logger, scopeProvicedMock);
		}


		private readonly Expression<Action<ILogsEventSource>> m_logExpression = e =>
			e.LogMessage(
				It.IsAny<string>(),
				It.IsAny<ActivityTraceId>(),
				It.IsAny<string>(),
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.IsAny<int>(),
				It.IsAny<string>());
	}
}
