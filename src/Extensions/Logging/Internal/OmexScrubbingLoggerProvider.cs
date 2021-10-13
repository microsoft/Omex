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
			m_logsEventSender = logsEventSender;
			m_defaultExternalScopeProvider = defaultExternalScopeProvider;
			m_replayer = replayer;
		}

		public ILogger CreateLogger(string categoryName) =>
			new OmexScrubbingLogger(m_logsEventSender, m_externalScopeProvider ?? m_defaultExternalScopeProvider, categoryName, m_replayer);

		public void Dispose() { }

		public void SetScopeProvider(IExternalScopeProvider scopeProvider) => m_externalScopeProvider = scopeProvider;

		private IExternalScopeProvider? m_externalScopeProvider;
		private readonly IExternalScopeProvider m_defaultExternalScopeProvider;
		private readonly ILogEventReplayer? m_replayer;
		private readonly ILogEventSender m_logsEventSender;
		private LogScrubber m_logScrubber = LogScrubber.Instance;
	}
}
