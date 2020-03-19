// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Logging.Replayable
{
	/// <summary>
	/// Replays activity logs if activity collects them
	/// </summary>
	internal interface ILogEventReplayer
	{
		/// <summary>
		/// Replays activity logs if activity supports it
		/// </summary>
		/// <param name="activity">Activity that should contain logs to replay</param>
		void ReplayLogs(Activity activity);
	}
}
