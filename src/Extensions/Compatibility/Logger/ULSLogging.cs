// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Compatibility.Logger
{
	/// <summary>
	/// Unified logging, allows registering for logging events sent by
	/// code implementations and to raise logging events
	/// </summary>
	public static class ULSLogging
	{
		/// <summary>
		/// Logs a trace
		/// </summary>
		/// <param name="eventId">Unique tag id for the trace</param>
		/// <param name="category">Logging category</param>
		/// <param name="level">Logging level</param>
		/// <param name="message">The message, or message format string, to log</param>
		/// <param name="parameters">Message format parameters</param>
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static void LogTraceTag(EventId eventId, Category category, LogLevel level, string message, params object[] parameters) =>
			GetLogger(category).Log(level, eventId, message, parameters);


		/// <summary>
		/// Report an exception
		/// </summary>
		/// <param name="eventId">Unique tag id for the trace</param>
		/// <param name="category">Logging category</param>
		/// <param name="exception">Exception to log</param>
		/// <param name="message">The message, or message format string, to log</param>
		/// <param name="parameters">Message format parameters</param>
		[Obsolete(ObsoleteMessage, IsObsoleteError)]
		public static void ReportExceptionTag(EventId eventId, Category category, Exception exception, string message, params object[] parameters) =>
			GetLogger(category).LogError(eventId, exception, message, parameters);


		internal static void Initialize(ILoggerFactory loggerFactory)
		{
			s_factory = loggerFactory;
			s_loggersDictionary = new ConcurrentDictionary<string, ILogger>(StringComparer.OrdinalIgnoreCase);
		}


		private static ILogger GetLogger(Category category) =>
			s_loggersDictionary != null && s_factory != null
			? s_loggersDictionary.GetOrAdd(category.Name, s_factory.CreateLogger)
			: throw new OmexCompatibilityInitializationException();


		private static ILoggerFactory? s_factory;
		private static ConcurrentDictionary<string, ILogger>? s_loggersDictionary;
		private const string ObsoleteMessage = "Please consider using ILogger instead";
		private const bool IsObsoleteError = false;
	}
}
