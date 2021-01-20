// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Compatibility
{
	/// <summary>
	/// Extension for string to start Activity
	/// </summary>
	public static class TimedScope
	{
		/// <summary>
		/// Creates Activity and starts it
		/// </summary>
		/// <param name="name">Activity name</param>
		/// <param name="initialResult">Initial result to use</param>
		[Obsolete("Please consider using ActivitySource directly by injecting it", false)]
		public static Activity? CreateAndStart(string name, ActivityResult initialResult)
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
