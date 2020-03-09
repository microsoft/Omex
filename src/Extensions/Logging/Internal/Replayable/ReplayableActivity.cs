// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Logging.Replayable
{
	internal class ReplayableActivity : Activity
	{
		public ReplayableActivity(string operationName, uint maxReplayedEventsPerActivity) : base(operationName)
		{
			m_maxReplayedEventsPerActivity = maxReplayedEventsPerActivity;
			m_logEvents = new ConcurrentQueue<LogMessageInformation>();
		}


		public void AddLogEvent(LogMessageInformation logEvent)
		{
			m_logEvents.Enqueue(logEvent);

			if (m_logEvents.Count > m_maxReplayedEventsPerActivity)
			{
				m_logEvents.TryDequeue(out _);
			}
		}


		public IEnumerable<LogMessageInformation> GetLogEvents() => m_logEvents;


		private readonly ConcurrentQueue<LogMessageInformation> m_logEvents;
		private readonly uint m_maxReplayedEventsPerActivity;
	}
}
