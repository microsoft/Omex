// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Interface for handling active scopes stack
	/// </summary>
	public interface ITimedScopeStackManager
	{
		/// <summary>
		/// Get a stack of active scopes, creating a new stack if one does not exist
		/// </summary>
		/// <returns>stack of scopes</returns>
		TimedScopeStack GetTimedScopeStack();


		/// <summary>
		/// Set stack of active scopes
		/// </summary>
		/// <param name="timedScopeStack">Timed scope stack</param>
		void SetTimedScopeStack(TimedScopeStack timedScopeStack);
	}
}
