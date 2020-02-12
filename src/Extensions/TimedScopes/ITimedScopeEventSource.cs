// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.TimedScopes
{
	/// <summary>Timed scope event source</summary>
	public interface ITimedScopeEventSource
	{
		/// <summary>Log timed scope end information</summary>
		/// <param name="scope">Ended timed scope</param>
		void LogTimedScopeEndEvent(TimedScope scope);
	}
}
