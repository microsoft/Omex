// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.TimedScopes.ReplayEventLogging
{
	/// <summary>
	/// Decides if a timed scope is disabled for event replaying
	/// </summary>
	public interface IReplayEventDisabledTimedScopes
	{
		/// <summary>
		/// Is the timed scope disabled for event replaying
		/// </summary>
		/// <param name="scopeDefinition">Scope definition</param>
		/// <returns><c>true</c> if the scope is disabled, <c>false</c> otherwise.</returns>
		bool IsDisabled(TimedScopeDefinition scopeDefinition);
	}
}
