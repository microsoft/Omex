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
		/// Log information about activity stop
		/// </summary>
		void LogActivityStop(Activity activity);
	}
}
