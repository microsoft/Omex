// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Omex.Extensions.Logging.Replayable;

namespace Microsoft.Omex.Extensions.Logging.Internal.Replayable
{
	internal static class ReplayebleActivityExtensions
	{
		private static readonly string s_replayableLogKey = "ReplayableLogKey";

		private static ConcurrentQueue<LogMessageInformation>? GetReplayableLogsInternal(this Activity activity) =>
			activity.GetCustomProperty(s_replayableLogKey) as ConcurrentQueue<LogMessageInformation>;

		public static IEnumerable<LogMessageInformation> GetReplayableLogs(this Activity activity) =>
			activity.GetReplayableLogsInternal() ?? Enumerable.Empty<LogMessageInformation>();

		public static void AddReplayLog(this Activity activity, LogMessageInformation logEvent, uint maxReplayedEventsPerActivity)
		{
			ConcurrentQueue<LogMessageInformation>? queue = activity.GetReplayableLogsInternal();
			if (queue == null)
			{
				queue = new ConcurrentQueue<LogMessageInformation>();
				activity.SetCustomProperty(s_replayableLogKey, queue);
			}

			queue.Enqueue(logEvent);

			if (queue.Count > maxReplayedEventsPerActivity)
			{
				queue.TryDequeue(out _);
			}
		}
	}
}
