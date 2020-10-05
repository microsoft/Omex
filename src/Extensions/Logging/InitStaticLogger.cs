// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// InitStaticLogger is the logger to be used before the uls logger is set
	/// not to be used as main logger
	/// </summary>
	public sealed class InitStaticLogger
	{
		private static ILogger s_logger = LoggerFactory.Create(builder =>
		{
			builder.LoadInitialLogger();
		}).CreateLogger("Initial-Logging");

		/// <summary>
		/// Log on success
		/// </summary>
		/// <param name="serviceNameForLogging">Service name for logging</param>
		/// <param name="message">Message to log</param>
		/// <param name="eventId">Event id</param>
		/// <param name="args">Extra arguments to be logged</param>
		public static void Succeed(string serviceNameForLogging, string message, EventId eventId, params object[] args)
		{
			ServiceInitializationEventSource eventSource = ServiceInitializationEventSource.Instance;
			eventSource.LogHostBuildSucceeded(Process.GetCurrentProcess().Id, serviceNameForLogging, message, args);
			s_logger.LogInformation(eventId, message, args);
		}

		/// <summary>
		/// Set the logger category
		/// </summary>
		/// <param name="category">Category</param>
		public static void SetLoggerCategory(string category)
		{
			s_logger = LoggerFactory.Create(builder =>
			{
				builder.LoadInitialLogger();
			}).CreateLogger(category);
		}

		/// <summary>
		/// Log on failure
		/// </summary>
		/// <param name="serviceNameForLogging">Service name for logging</param>
		/// <param name="ex">Exception to log</param>
		/// <param name="message">Message to log</param>
		/// <param name="eventId">Event id</param>
		/// <param name="args">Extra arguments to be logged</param>
		public static void Fail(string serviceNameForLogging, string message, EventId eventId, Exception? ex, params object[] args)
		{
			ServiceInitializationEventSource eventSource = ServiceInitializationEventSource.Instance;
			eventSource.LogHostFailed(ex?.Message ?? string.Empty, serviceNameForLogging, message, args);
			s_logger.LogInformation(eventId, ex, message, args);
		}
	}
}
