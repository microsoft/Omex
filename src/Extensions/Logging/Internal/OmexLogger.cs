// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Logging.Replayable;

namespace Microsoft.Omex.Extensions.Logging
{
	internal class OmexLogger : ILogger
	{
		public OmexLogger(ILogEventSender logsEventSource, IExternalScopeProvider externalScopeProvider, string categoryName)
		{
			m_logsEventSender = logsEventSource;
			m_externalScopeProvider = externalScopeProvider;
			m_categoryName = categoryName;
		}

		public IDisposable BeginScope<TState>(TState state) => m_externalScopeProvider.Push(state);

		public bool IsEnabled(LogLevel logLevel) => m_logsEventSender.IsEnabled(logLevel);

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			string message = formatter(state, exception);
			if (exception != null)
			{
				message = string.Concat(message, Environment.NewLine, exception); // We need to concatenate with exception since the default formatter ignores it https://github.com/aspnet/Logging/issues/442
			}

			int threadId = Thread.CurrentThread.ManagedThreadId;
			Activity? activity = Activity.Current;

			m_logsEventSender.LogMessage(activity, m_categoryName, logLevel, eventId, threadId, message, exception);

			if (m_logsEventSender.IsReplayableMessage(logLevel) && activity is ReplayableActivity replayableScope)
			{
				replayableScope.AddLogEvent(new LogMessageInformation(m_categoryName, eventId, threadId, message, exception));
			}
		}

		private readonly IExternalScopeProvider m_externalScopeProvider;
		private readonly ILogEventSender m_logsEventSender;
		private readonly string m_categoryName;
	}
}
