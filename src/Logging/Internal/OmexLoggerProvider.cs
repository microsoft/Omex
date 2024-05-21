// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Logging.Replayable;
using Microsoft.Omex.Extensions.Logging.Scrubbing;

namespace Microsoft.Omex.Extensions.Logging
{
	[ProviderAlias("Omex")]
	[Obsolete($"{nameof(OmexLogger)} and {nameof(OmexLogEventSource)} are obsolete and pending for removal by 1 July 2024. Please consider using a different logging solution.", DiagnosticId = "OMEX188")]
	internal class OmexLoggerProvider : ILoggerProvider, ISupportExternalScope
	{
		public OmexLoggerProvider(
			ILogEventSender logsEventSender,
			IExternalScopeProvider defaultExternalScopeProvider,
			IEnumerable<ILogScrubbingRule> textScrubbers,
			IOptionsMonitor<OmexLoggingOptions> options,
			ILogEventReplayer? replayer = null)
		{
			m_logsEventSender = logsEventSender;
			m_defaultExternalScopeProvider = defaultExternalScopeProvider;
			m_textScrubbers = textScrubbers;
			m_options = options;
			m_replayer = replayer;
		}

		public ILogger CreateLogger(string categoryName) =>
			new OmexLogger(m_logsEventSender, m_externalScopeProvider ?? m_defaultExternalScopeProvider, m_textScrubbers, categoryName, m_options.CurrentValue, m_replayer);

		public void Dispose() { }

		public void SetScopeProvider(IExternalScopeProvider scopeProvider) => m_externalScopeProvider = scopeProvider;

		private IExternalScopeProvider? m_externalScopeProvider;

		private readonly ILogEventSender m_logsEventSender;
		private readonly IExternalScopeProvider m_defaultExternalScopeProvider;
		private readonly IEnumerable<ILogScrubbingRule> m_textScrubbers;
		private readonly IOptionsMonitor<OmexLoggingOptions> m_options;
		private readonly ILogEventReplayer? m_replayer;
	}
}
