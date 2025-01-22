﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// InitializationLogger is the logger to be used before the proper ILogger from DI is set.
	/// Not to be used as main logger.
	/// </summary>
	public static class InitializationLogger
	{
		/// <summary>
		/// Instance of logger
		/// </summary>
		public static ILogger Instance { get; private set; } = LoggerFactory.Create(builder =>
		{
			if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
			{
				builder.AddConsole();
			}

			s_loggingBuilderAction?.Invoke(builder);
		}).CreateLogger("Initial-Logging");

		/// <summary>
		/// Action to customize the initialization logger builder
		/// </summary>
		/// <param name="action"></param>
		public static void CustomizeInitializationLoggerBuilder(Action<ILoggingBuilder> action) => s_loggingBuilderAction = action;

		private static Action<ILoggingBuilder>? s_loggingBuilderAction;

		/// <summary>
		/// Log on successful building
		/// </summary>
		/// <param name="serviceNameForLogging">Service name for logging</param>
		/// <param name="message">Message to log</param>
		public static void LogInitializationSucceed(string serviceNameForLogging, string message = "")
		{
			string logMessage = $"Initialization successful for {serviceNameForLogging}, {message}";
#if NET5_0_OR_GREATER
			ServiceInitializationEventSource.Instance.LogHostBuildSucceeded(Environment.ProcessId, serviceNameForLogging, logMessage);
#else
			using Process process = Process.GetCurrentProcess();
			ServiceInitializationEventSource.Instance.LogHostBuildSucceeded(process.Id, serviceNameForLogging, logMessage);
#endif
			Instance.LogInformation(Tag.Create(), logMessage);
		}

		/// <summary>
		/// Log on build failure
		/// </summary>
		/// <param name="serviceNameForLogging">Service name for logging</param>
		/// <param name="ex">Exception to log</param>
		/// <param name="message">Message to log</param>
		public static void LogInitializationFail(string serviceNameForLogging, Exception? ex = null, string message = "")
		{
			ServiceInitializationEventSource.Instance.LogHostFailed(ex?.ToString() ?? string.Empty, serviceNameForLogging, message);
			Instance.LogError(Tag.Create(), ex, message);
		}
	}
}
