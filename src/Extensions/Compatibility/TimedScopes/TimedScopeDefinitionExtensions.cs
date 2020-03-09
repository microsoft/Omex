// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.TimedScopes;

namespace Microsoft.Omex.Extensions.Compatibility.TimedScopes
{
	/// <summary>
	/// Extension for TimedScopeDefinition
	/// </summary>
	public static class TimedScopeDefinitionExtensions
	{
		/// <summary>
		/// Creates a scope (and starts by default)
		/// </summary>
		/// <param name="timedScopeDefinition">TimedScopeDefinition to use</param>
		/// <param name="initialResult">Initial result to use</param>
		/// <param name="startScope">Should the scope be automatically started (for use in e.g. 'using' statement)</param>
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static TimedScope Create(this TimedScopeDefinition timedScopeDefinition, TimedScopeResult initialResult, bool startScope = true)
		{
			if (s_timedScopeProvider == null)
			{
				throw new OmexCompatibilityInitializationException();
			}

			TimedScope scope = s_timedScopeProvider.Create(timedScopeDefinition, initialResult);

			if (startScope)
			{
				scope.Start();
			}

			return scope;
		}


		internal static void Initialize(ITimedScopeProvider provider) => s_timedScopeProvider = provider;


		private static ITimedScopeProvider? s_timedScopeProvider;
		private const string ObsoleteMessage = "Please consider using ITimedScopeProvider directly by injecting it";
		private const bool IsObsoleteError = false;
	}
}
