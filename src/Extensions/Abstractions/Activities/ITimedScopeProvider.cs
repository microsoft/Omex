// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Abstractions.Activities
{
	/// <summary>
	/// Interface to create TimedScope
	/// </summary>
	public interface ITimedScopeProvider
	{
		/// <summary>
		/// Creates and start TimedScope
		/// </summary>
		TimedScope CreateAndStart(TimedScopeDefinition name, TimedScopeResult result = TimedScopeResult.SystemError);

		/// <summary>
		/// Creates TimedScope
		/// </summary>
		TimedScope Create(TimedScopeDefinition name, TimedScopeResult result = TimedScopeResult.SystemError);
	}
}
