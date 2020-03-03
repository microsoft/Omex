// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Logging
{
	internal class OmexLoggerProvider : ILoggerProvider, ISupportExternalScope
	{
		public OmexLoggerProvider(ILogEventSender logsEventSource, IExternalScopeProvider defaultExternalScopeProvider)
		{
			m_logsEventSource = logsEventSource;
			m_defaultExternalScopeProvider = defaultExternalScopeProvider;
		}


		public ILogger CreateLogger(string categoryName) => new OmexLogger(m_logsEventSource, m_externalScopeProvider ?? m_defaultExternalScopeProvider, categoryName);


		public void Dispose() { }


		public void SetScopeProvider(IExternalScopeProvider scopeProvider) => m_externalScopeProvider = scopeProvider;


		private IExternalScopeProvider? m_externalScopeProvider;
		private readonly IExternalScopeProvider m_defaultExternalScopeProvider;
		private readonly ILogEventSender m_logsEventSource;
	}
}
