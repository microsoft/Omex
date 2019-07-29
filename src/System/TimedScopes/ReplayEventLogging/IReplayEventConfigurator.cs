// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.TimedScopes.ReplayEventLogging
{
	/// <summary>
	/// Configures event replaying when a timed scope ends
	/// </summary>
	public interface IReplayEventConfigurator
	{
		/// <summary>
		/// Configure event replaying when a timed scope ends
		/// </summary>
		/// <param name="scope"></param>
		void ConfigureReplayEventsOnScopeEnd(TimedScope scope);
	}
}
