﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions.Scrubbing;
using Microsoft.Omex.Extensions.Logging.Replayable;

namespace Microsoft.Omex.Extensions.Logging
{
	internal class OmexLoggerProvider : ILoggerProvider, ISupportExternalScope
	{
		public OmexLoggerProvider(ILogEventSender logsEventSender, IExternalScopeProvider defaultExternalScopeProvider, ITextScrubber textScrubber, ILogEventReplayer? replayer = null)
		{
			m_logsEventSender = logsEventSender;
			m_defaultExternalScopeProvider = defaultExternalScopeProvider;
			m_textScrubber = textScrubber;
			m_replayer = replayer;
		}

		public ILogger CreateLogger(string categoryName) =>
			new OmexLogger(m_logsEventSender, m_externalScopeProvider ?? m_defaultExternalScopeProvider, m_textScrubber, categoryName, m_replayer);

		public void Dispose() { }

		public void SetScopeProvider(IExternalScopeProvider scopeProvider) => m_externalScopeProvider = scopeProvider;

		private IExternalScopeProvider? m_externalScopeProvider;
		private readonly IExternalScopeProvider m_defaultExternalScopeProvider;
		private readonly ITextScrubber m_textScrubber;
		private readonly ILogEventReplayer? m_replayer;
		private readonly ILogEventSender m_logsEventSender;
	}
}
