// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	internal sealed class TimedScopeProvider : ITimedScopeProvider
	{
		public TimedScopeProvider(ITimedScopeEventSource eventSource, IActivityProvider activityProvider, ILogReplayer? logReplayer = null)
		{
			m_activityProvider = activityProvider;
			m_eventSource = eventSource;
			m_logReplayer = logReplayer;
		}


		public TimedScope Start(string name, TimedScopeResult result) =>
			new TimedScope(m_eventSource, m_activityProvider.Create(name), result, m_logReplayer);


		private readonly ITimedScopeEventSource m_eventSource;
		private readonly IActivityProvider m_activityProvider;
		private readonly ILogReplayer? m_logReplayer;
	}
}
