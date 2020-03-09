// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	/// <summary>
	/// Interface to create TimedScope
	/// </summary>
	public interface ITimedScopeProvider
	{
		/// <summary>
		/// Creates and start TimedScope
		/// </summary>
		TimedScope CreateAndStart(TimedScopeDefinition name, TimedScopeResult result);


		/// <summary>
		/// Creates TimedScope
		/// </summary>
		TimedScope Create(TimedScopeDefinition name, TimedScopeResult result);
	}
}
