// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Services.Remoting
{
	internal static class Diagnostics
	{
		internal const string AssemblyName = "Microsoft.Omex.Extensions.Services.Remoting";

		/// <summary>
		/// Name of the exception event
		/// </summary>
		/// <remarks>
		/// Should end with Exception to be enabled by out telemetry
		/// </remarks>
		private const string ExceptionEventName = AssemblyName + ".Exception";

		private static DiagnosticListener s_diagnosticListener = new DiagnosticListener(AssemblyName);

		public static Activity? StartActivity(string name)
		{
			if (!s_diagnosticListener.IsEnabled(name))
			{
				return null;
			}

			Activity activity = new Activity(name);
			return s_diagnosticListener.StartActivity(activity, null);
		}

		public static void StopActivity(Activity? activity)
		{
			if (activity != null)
			{
				s_diagnosticListener.StopActivity(activity, null);
			}
		}

		public static void ReportException(Exception exception)
		{
			if (s_diagnosticListener.IsEnabled(ExceptionEventName))
			{
				s_diagnosticListener.Write(ExceptionEventName, exception);
			}
		}
	}
}
