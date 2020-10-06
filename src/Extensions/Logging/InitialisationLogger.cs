// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// InitStaticLogger is the logger to be used before the proper ILogger from DI is set
	/// not to be used as main logger
	/// </summary>
	public static class InitialisationLogger
	{
		/// <summary>
		/// Instance of logger
		/// </summary>
		public static ILogger Instance { get; private set; } = LoggerFactory.Create(builder =>
		 {
			 builder.LoadInitialisationLogger();
		 }).CreateLogger("Initial-Logging");

		/// <summary>
		/// Log on success
		/// </summary>
		/// <param name="serviceNameForLogging">Service name for logging</param>
		/// <param name="message">Message to log</param>
		public static void InitilisationSucceed(string serviceNameForLogging, string message = "")
		{
			string logMessage = string.IsNullOrWhiteSpace(message) ?
				$"Initilisation logger logging success to {serviceNameForLogging}" : message ;
			ServiceInitializationEventSource.Instance.LogHostBuildSucceeded(Process.GetCurrentProcess().Id, serviceNameForLogging, logMessage);
			Instance.LogInformation(logMessage);
		}

		/// <summary>
		/// Log on failure
		/// </summary>
		/// <param name="serviceNameForLogging">Service name for logging</param>
		/// <param name="ex">Exception to log</param>
		/// <param name="message">Message to log</param>
		public static void InitilisationFail(string serviceNameForLogging,  Exception? ex = null, string message = "")
		{
			ServiceInitializationEventSource.Instance.LogHostFailed(ex?.Message ?? string.Empty, serviceNameForLogging, message);
			Instance.LogInformation(ex, message);
		}
	}
}
