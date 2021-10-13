// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Logging.Replayable;
using Microsoft.Omex.Extensions.Logging.Scrubbing;

namespace Microsoft.Omex.Extensions.Logging.Internal.EventSource
{
	internal class OmexScrubbingLoggerProvider
		: ILoggerProvider, ISupportExternalScope
	{
		public OmexScrubbingLoggerProvider(ILogEventSender logsEventSender, IExternalScopeProvider defaultExternalScopeProvider, ILogEventReplayer? replayer = null)
		{
			m_loggerProvider = new OmexLoggerProvider(logsEventSender, defaultExternalScopeProvider, replayer);
		}

		public ILogger CreateLogger(string categoryName) =>
			m_loggerProvider!.CreateLogger(m_logScrubber.Scrub(categoryName));

		public void Dispose() => m_loggerProvider!.Dispose();
		public void SetScopeProvider(IExternalScopeProvider scopeProvider) => m_loggerProvider!.SetScopeProvider(scopeProvider);

		private OmexLoggerProvider? m_loggerProvider;
		private LogScrubber m_logScrubber = LogScrubber.Instance;
	}
}
