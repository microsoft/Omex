// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>Replays activity logs if activity collects them</summary>
	public interface ILogReplayer
	{
		/// <summary>
		/// Replays activity logs if activity supports it
		/// </summary>
		/// <param name="activity"></param>
		void ReplayLogs(Activity activity);
	}
}
