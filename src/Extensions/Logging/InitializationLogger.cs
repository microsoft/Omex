// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// InitStaticLogger is the logger to be used before the proper ILogger from DI is set.
	/// Not to be used as main logger.
	/// </summary>
	public static class InitializationLogger
	{
		/// <summary>
		/// Instance of logger
		/// </summary>
		public static ILogger Instance { get; private set; } = LoggerFactory.Create(builder =>
		 {
			builder.LoadInitializationLogger();
		 }).CreateLogger("Initial-Logging");

		private static ILoggingBuilder LoadInitializationLogger(this ILoggingBuilder builder)
		{
			builder.AddConsole();
			builder.AddOmexLogging();
			return builder;
		}

		/// <summary>
		/// Log on successful building
		/// </summary>
		/// <param name="serviceNameForLogging">Service name for logging</param>
		/// <param name="message">Message to log</param>
		public static void InitilizationSucceed(string serviceNameForLogging, string message = "")
		{
			string logMessage = $"Initilization successful for {serviceNameForLogging}, {message}";
			ServiceInitializationEventSource.Instance.LogHostBuildSucceeded(Process.GetCurrentProcess().Id, serviceNameForLogging, logMessage);
			Instance.LogInformation(Tag.Create(), logMessage);
		}

		/// <summary>
		/// Log on build failure
		/// </summary>
		/// <param name="serviceNameForLogging">Service name for logging</param>
		/// <param name="ex">Exception to log</param>
		/// <param name="message">Message to log</param>
		public static void InitilizationFail(string serviceNameForLogging,  Exception? ex = null, string message = "")
		{
			ServiceInitializationEventSource.Instance.LogHostFailed(ex?.ToString() ?? string.Empty, serviceNameForLogging, message);
			Instance.LogError(Tag.Create(), ex, message);
		}
	}
}
