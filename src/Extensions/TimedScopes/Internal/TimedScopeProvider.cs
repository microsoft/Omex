// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	internal sealed class TimedScopeProvider : ITimedScopeProvider
	{
		public TimedScopeProvider(
			ITimedScopeEventSource eventSource,
			IActivityProvider activityProvider,
			ILogReplayer? logReplayer = null)
		{
			m_activityProvider = activityProvider;
			m_eventSource = eventSource;
			m_logReplayer = logReplayer;
		}


		public TimedScope Start(string name, TimedScopeResult result)
		{
			// Activity won't be stated in case of an empty string
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("TimedScope name should not be empty or null");
			}
			return new TimedScope(m_eventSource, m_activityProvider.Create(name), result, m_logReplayer).Start();
		}

		private readonly ITimedScopeEventSource m_eventSource;
		private readonly IActivityProvider m_activityProvider;
		private readonly ILogReplayer? m_logReplayer;
	}
}
