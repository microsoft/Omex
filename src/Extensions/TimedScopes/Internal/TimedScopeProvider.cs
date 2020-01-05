// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Logging.TimedScopes
{
	internal class TimedScopeProvider : ITimedScopeProvider
	{
		public TimedScopeProvider(IMachineInformation machineInformation, TimedScopeEventSource eventSource, IActivityProvider activityProvider, ILogReplayer? logReplayer = null)
		{
			m_serviceName = machineInformation.ServiceName;
			m_activityProvider = activityProvider;
			m_eventSource = eventSource;
			m_logReplayer = logReplayer;
			ReplayEventsInCaseOfError = logReplayer != null;
		}


		public TimedScope Start(string name, TimedScopeResult result) =>
			new TimedScope(m_eventSource, m_activityProvider.Create(name, ReplayEventsInCaseOfError), m_serviceName, result, m_logReplayer);


		public bool ReplayEventsInCaseOfError { get; set; }
		private readonly TimedScopeEventSource m_eventSource;
		private readonly IActivityProvider m_activityProvider;
		private readonly string m_serviceName;
		private readonly ILogReplayer? m_logReplayer;
	}
}
