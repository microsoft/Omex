// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Abstractions.ReplayableLogs
{
	/// <summary>
	/// Replays activity logs if activity collects them
	/// </summary>
	public interface ILogEventReplayer
	{
		/// <summary>
		/// Replays activity logs if activity supports it
		/// </summary>
		/// <param name="activity">Activity that should contain logs to replay</param>
		void ReplayLogs(Activity activity);
	}
}
