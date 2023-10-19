// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Logging.Replayable;
using Microsoft.Omex.Extensions.Logging.Scrubbing;

namespace Microsoft.Omex.Extensions.Logging
{
	[ProviderAlias("Omex")]
	internal class OmexLoggerProvider : ILoggerProvider, ISupportExternalScope
	{
		public OmexLoggerProvider(
			ILogEventSender logsEventSender,
			IExternalScopeProvider defaultExternalScopeProvider,
			IEnumerable<ILogScrubbingRule> textScrubbers,
			ILogEventReplayer? replayer = null)
		{
			m_logsEventSender = logsEventSender;
			m_defaultExternalScopeProvider = defaultExternalScopeProvider;
			m_textScrubbers = textScrubbers;
			m_replayer = replayer;
		}

		public ILogger CreateLogger(string categoryName) =>
			return new OmexLogger(m_logsEventSender, m_externalScopeProvider ?? m_defaultExternalScopeProvider, m_textScrubbers, categoryName, m_replayer);

		public void Dispose() { }

		public void SetScopeProvider(IExternalScopeProvider scopeProvider) => m_externalScopeProvider = scopeProvider;

		private IExternalScopeProvider? m_externalScopeProvider;

		private readonly ILogEventSender m_logsEventSender;
		private readonly IExternalScopeProvider m_defaultExternalScopeProvider;
		private readonly IEnumerable<ILogScrubbingRule> m_textScrubbers;
		private readonly ILogEventReplayer? m_replayer;
	}
}
