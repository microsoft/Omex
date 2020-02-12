// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.TimedScopes;

namespace Microsoft.Omex.Extensions.Logging.TimedScopes
{
	internal class TimedScopeProvider : ITimedScopeProvider
	{
		public TimedScopeProvider(ITimedScopeEventSource eventSource, IActivityProvider activityProvider, ILogReplayer? logReplayer = null)
		{
			m_activityProvider = activityProvider;
			m_eventSource = eventSource;
			m_logReplayer = logReplayer;
			m_replayEventsInCaseOfError = logReplayer != null;
		}


		public TimedScope Start(string name, TimedScopeResult result) =>
			new TimedScope(m_eventSource, m_activityProvider.Create(name, m_replayEventsInCaseOfError), result, m_logReplayer);


		private bool m_replayEventsInCaseOfError;
		private readonly ITimedScopeEventSource m_eventSource;
		private readonly IActivityProvider m_activityProvider;
		private readonly ILogReplayer? m_logReplayer;
	}
}
