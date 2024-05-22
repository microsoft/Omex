// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Logging.Replayable;
using Microsoft.Omex.Extensions.Logging.Scrubbing;

namespace Microsoft.Omex.Extensions.Logging
{
	internal class OmexLogger : ILogger
	{
		public OmexLogger(
			ILogEventSender logsEventSource,
			IExternalScopeProvider externalScopeProvider,
			IEnumerable<ILogScrubbingRule> textScrubbers,
			string categoryName,
			OmexLoggingOptions omexLoggingOptions,
			ILogEventReplayer? replayer = null)
		{
			m_logsEventSender = logsEventSource;
			m_externalScopeProvider = externalScopeProvider;
			m_textScrubbers = textScrubbers.ToArray(); // Convert to an array for improved iteration performance on each call.
			m_categoryName = categoryName;
			m_omexLoggingOptions = omexLoggingOptions;
			m_replayer = replayer;
		}

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => m_externalScopeProvider.Push(state);

		public bool IsEnabled(LogLevel logLevel) => m_omexLoggingOptions.OmexLoggerEnabled && m_logsEventSender.IsEnabled(logLevel);

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

			foreach (ILogScrubbingRule textScrubber in m_textScrubbers)
			{
				message = textScrubber.Scrub(message);
			}

			int threadId = Thread.CurrentThread.ManagedThreadId;
			Activity? activity = Activity.Current;

			m_logsEventSender.LogMessage(activity, m_categoryName, logLevel, eventId, threadId, message, exception);

			if (activity != null
				&& m_replayer != null
				&& m_replayer.IsReplayableMessage(logLevel))
			{
				m_replayer.AddReplayLog(activity, new LogMessageInformation(m_categoryName, eventId, threadId, message, exception));
			}
		}

		private readonly IExternalScopeProvider m_externalScopeProvider;
		private readonly ILogEventSender m_logsEventSender;
		private readonly ILogScrubbingRule[] m_textScrubbers;
		private readonly string m_categoryName;
		private readonly OmexLoggingOptions m_omexLoggingOptions;
		private readonly ILogEventReplayer? m_replayer;
	}
}
