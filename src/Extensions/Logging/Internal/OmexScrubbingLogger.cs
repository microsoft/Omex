// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Logging.Replayable;
using Microsoft.Omex.Extensions.Logging.Scrubbing;

namespace Microsoft.Omex.Extensions.Logging.Internal.EventSource
{
	internal class OmexScrubbingLogger
		 : ILogger
	{
		public OmexScrubbingLogger(ILogEventSender logsEventSource,
			IExternalScopeProvider externalScopeProvider,
			string categoryName,
			ILogEventReplayer? replayer = null)
		{
			m_logger = new OmexLogger(logsEventSource, externalScopeProvider, categoryName, replayer);
		}

		public IDisposable BeginScope<TState>(TState state) => m_logger!.BeginScope<TState>(state);

		public bool IsEnabled(LogLevel logLevel) => m_logger!.IsEnabled(logLevel);

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception?, string> formatter) {
			string? message = string.Empty;
			if (state != null)
			{
				#pragma warning disable CS8604 // Possible null reference argument.
				message = m_logScrubber.Scrub(state!.ToString());
				#pragma warning restore CS8604 // Possible null reference argument.
			}

			if (exception != null)
			{
				message = string.Concat(message, Environment.NewLine, exception);
			} 

			m_logger!.Log<string>(logLevel, eventId, message, null, (string stateToFormat, Exception? exception) => stateToFormat);
		}

		private OmexLogger? m_logger;
		private LogScrubber m_logScrubber = LogScrubber.Instance;
	}
}
