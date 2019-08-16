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
		/// Get a stack of active scopes, create a new stack if one does not exist
		/// </summary>
		/// <returns>stack of scopes</returns>
		TimedScopeStack Scopes {get; set;}
	}
}
