// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Compatibility.TimedScopes
{
	/// <summary>
	/// Extension for TimedScopeDefinition
	/// </summary>
	public static class TimedScope
	{
		/// <summary>
		/// Creates a scope (and starts by default)
		/// </summary>
		/// <param name="name">TimedScopeDefinition to use</param>
		/// <param name="initialResult">Initial result to use</param>
		[Obsolete("Please consider using ITimedScopeProvider directly by injecting it", false)]
		public static Activity? CreateAndStart(this string name, ActivityResult initialResult)
		{
			if (s_activitySource == null)
			{
				throw new OmexCompatibilityInitializationException();
			}

			Activity? scope = s_activitySource.StartActivity(name);

			scope?.SetResult(initialResult);

			return scope;
		}

		internal static void Initialize(ActivitySource provider) => s_activitySource = provider;

		private static ActivitySource? s_activitySource;
	}
}
