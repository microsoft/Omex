// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	/// <summary>
	/// Timed scope event source
	/// </summary>
	public interface ITimedScopeEventSender
	{
		/// <summary>
		/// Log timed scope end information
		/// </summary>
		void LogActivityStop(Activity activity);
	}
}
