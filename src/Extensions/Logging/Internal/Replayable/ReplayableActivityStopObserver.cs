// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.Logging.Replayable
{
	internal class ReplayableActivityStopObserver : IActivityStopObserver
	{
		private readonly ILogEventReplayer m_logReplayer;
		private readonly IOptions<OmexLoggingOptions> m_options;

		public ReplayableActivityStopObserver(ILogEventReplayer logReplayer, IOptions<OmexLoggingOptions> options)
		{
			m_logReplayer = logReplayer;
			m_options = options;
		}

		public void OnStop(Activity activity, object? payload)
		{
			if (m_options.Value.ReplayLogsInCaseOfError && ShouldReplayEvents(activity))
			{
				m_logReplayer.ReplayLogs(activity);
			}
		}

		private bool ShouldReplayEvents(Activity activity)
		{
			string? resultTagValue = activity.Tags.FirstOrDefault(p => string.Equals(p.Key, ActivityTagKeys.Result, StringComparison.Ordinal)).Value;
			return string.Equals(ActivityResultStrings.SystemError, resultTagValue, StringComparison.Ordinal);
		}
	}
}
