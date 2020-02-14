// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Logging.Replayable
{
	internal class LogRepayer : ILogReplayer
	{
		public LogRepayer(OmexLogsEventSource omexLogsEventSource) =>
			m_eventSource = omexLogsEventSource;


		public void ReplayLogs(Activity activity)
		{
			if (activity is ReplayableActivity replayableActivity)
			{
				foreach (LogMessageInformation log in replayableActivity.GetLogEvents())
				{
					m_eventSource.ReplayEvent(replayableActivity, log);
				}
			}
		}


		private readonly OmexLogsEventSource m_eventSource;
	}
}
