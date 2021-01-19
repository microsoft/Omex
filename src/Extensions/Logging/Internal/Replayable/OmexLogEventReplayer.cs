// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Logging.Internal.Replayable;
using Microsoft.Omex.Extensions.Logging.Replayable;

namespace Microsoft.Omex.Extensions.Logging
{
	internal class OmexLogEventReplayer : ILogEventReplayer
	{
		private readonly ILogEventSender m_sender;
		private readonly IOptionsMonitor<OmexLoggingOptions> m_options;

		public OmexLogEventReplayer(ILogEventSender sender, IOptionsMonitor<OmexLoggingOptions> options)
		{
			m_sender = sender;
			m_options = options;
		}

		public bool IsReplayableMessage(LogLevel logLevel) =>
			logLevel switch
			{
				LogLevel.Trace or LogLevel.Debug => m_options.CurrentValue.ReplayLogsInCaseOfError,
				_ => false
			};

		public void AddReplayLog(Activity activity, LogMessageInformation logMessage) =>
			activity.AddReplayLog(logMessage, m_options.CurrentValue.MaxReplayedEventsPerActivity);

		public void ReplayLogs(Activity activity)
		{
			// Replay parent activity
			if (activity.Parent != null)
			{
				ReplayLogs(activity.Parent);
			}

			foreach (LogMessageInformation log in activity.GetReplayableLogs())
			{
				m_sender.LogMessage(activity, log.Category, LogLevel.Information, log.EventId, log.ThreadId, log.Message, log.Exception);
			}
		}
	}
}
