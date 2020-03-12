// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.ReplayableLogs;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	internal sealed class TimedScopeProvider : ITimedScopeProvider
	{
		public TimedScopeProvider(
			ITimedScopeEventSender eventSource,
			IActivityProvider activityProvider,
			ILogEventReplayer? logReplayer = null)
		{
			m_activityProvider = activityProvider;
			m_eventSender = eventSource;
			m_logReplayer = logReplayer;
		}


		public TimedScope CreateAndStart(TimedScopeDefinition name, TimedScopeResult result) =>
			Create(name, result).Start();


		public TimedScope Create(TimedScopeDefinition name, TimedScopeResult result) =>
			new TimedScope(m_eventSender, m_activityProvider.Create(name), result, m_logReplayer);


		private readonly ITimedScopeEventSender m_eventSender;
		private readonly IActivityProvider m_activityProvider;
		private readonly ILogEventReplayer? m_logReplayer;
	}
}
