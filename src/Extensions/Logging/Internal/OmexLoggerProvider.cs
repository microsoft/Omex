// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Logging.Replayable;
using Microsoft.Omex.Extensions.Logging.Scrubbing;

namespace Microsoft.Omex.Extensions.Logging
{
	internal class OmexLoggerProvider : ILoggerProvider, ISupportExternalScope
	{
		public OmexLoggerProvider(
			ILogEventSender logsEventSender,
			IExternalScopeProvider defaultExternalScopeProvider,
			ILogEventReplayer? replayer = null,
			IEnumerable<ILogScrubbingRule>? textScrubbers = null)
		{
			m_logsEventSender = logsEventSender;
			m_defaultExternalScopeProvider = defaultExternalScopeProvider;
			m_replayer = replayer;
			m_textScrubbers = textScrubbers;
		}

		public ILogger CreateLogger(string categoryName) =>
			new OmexLogger(m_logsEventSender, m_externalScopeProvider ?? m_defaultExternalScopeProvider, categoryName, m_replayer, m_textScrubbers);

		public void Dispose() { }

		public void SetScopeProvider(IExternalScopeProvider scopeProvider) => m_externalScopeProvider = scopeProvider;

		private IExternalScopeProvider? m_externalScopeProvider;
		private readonly IExternalScopeProvider m_defaultExternalScopeProvider;
		private readonly ILogEventReplayer? m_replayer;
		private readonly ILogEventSender m_logsEventSender;
		private readonly IEnumerable<ILogScrubbingRule>? m_textScrubbers;
	}
}
