// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Logging.Replayable
{
	/// <summary>
	/// Replays activity logs
	/// </summary>
	internal interface ILogEventReplayer
	{
		void ReplayLogs(Activity activity);

		bool IsReplayableMessage(LogLevel logLevel);

		void AddReplayLog(Activity activity, LogMessageInformation logMessage);
	}
}
