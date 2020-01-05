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
		public OmexLogger(OmexLogsEventSource logsEventSource, IExternalScopeProvider externalScopeProvider, string categoryName)
		{
			m_logsEventSource = logsEventSource;
			m_externalScopeProvider = externalScopeProvider;
			m_categoryName = categoryName;
		}


		public IDisposable BeginScope<TState>(TState state) => m_externalScopeProvider.Push(state);


		public bool IsEnabled(LogLevel logLevel) => m_logsEventSource.IsEnabled(logLevel);


		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (IsEnabled(logLevel))
			{
				return;
			}

			string message = formatter(state, exception);
			int threadId = Thread.CurrentThread.ManagedThreadId;
			Activity activity = Activity.Current;
			string activityId = activity?.Id ?? string.Empty;
			ActivityTraceId traceId = activity?.TraceId ?? default;

			m_logsEventSource.ServiceMessage(activityId, traceId, m_categoryName, logLevel, eventId, threadId, message);

			if (m_logsEventSource.IsReplayableMessage(logLevel) && activity is ReplayableActivity replayableyScope)
			{
				replayableyScope.AddLogEvent(new LogMessageInformation(m_categoryName, eventId, threadId, message));
			}
		}


		private readonly IExternalScopeProvider m_externalScopeProvider;
		private readonly OmexLogsEventSource m_logsEventSource;
		private readonly string m_categoryName;
	}
}
