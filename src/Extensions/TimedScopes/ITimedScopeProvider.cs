// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.TimedScopes
{
	/// <summary>
	/// Interface to create TimedScope
	/// </summary>
	public interface ITimedScopeProvider
	{
		/// <summary>
		/// Create and start TimedScope
		/// </summary>
		TimedScope Start(string name, TimedScopeResult result);
	}
}
