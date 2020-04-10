// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Services.Remoting
{
	internal static class Diagnostics
	{
		/// <summary>
		/// Name of the listener that library use to create diagnostic events
		/// </summary>
		/// <remarks>
		/// Changing this name will mean that consumers might miss telemetry events
		/// </remarks>
		internal const string DiagnosticListenerName = "Microsoft.Omex.Extensions.Services.Remoting";

		/// <summary>
		/// Name of the exception event
		/// </summary>
		/// <remarks>
		/// Should end with Exception to be enabled by our telemetry
		/// </remarks>
		private const string ExceptionEventName = DiagnosticListenerName + ".Exception";

		public static DiagnosticListener DefaultListener { get; } = new DiagnosticListener(DiagnosticListenerName);

		public static Activity? CreateAndStartActivity(this DiagnosticListener listener, string name)
		{
			if (!listener.IsEnabled(name))
			{
				return null;
			}

			Activity activity = new Activity(name);
			return listener.StartActivity(activity, null);
		}

		public static void StopActivityIfExist(this DiagnosticListener listener, Activity? activity)
		{
			if (activity != null)
			{
				listener.StopActivity(activity, null);
			}
		}

		public static void ReportException(this DiagnosticListener listener, Exception exception)
		{
			if (listener.IsEnabled(ExceptionEventName))
			{
				listener.Write(ExceptionEventName, exception);
			}
		}
	}
}
