// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Timed scope provider
	/// </summary>
	public interface ITimedScopeProvider
	{
		/// <summary>
		/// Create timed scope
		/// </summary>
		/// <param name="definition">Timed scope definition</param>
		/// <param name="initialResult">Intial result of timed scope</param>
		/// <param name="startScope">Should create timed scope be started after creation</param>
		/// <returns></returns>
		TimedScope Create(TimedScopeDefinition definition, TimedScopeResult initialResult, bool startScope = true);
	}
}
