// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Omex.System.Logging;

namespace Microsoft.Omex.System.Configuration.DataSets
{
	/// <summary>
	/// Wrapper for ULSLogging that logs to dataset errors list
	/// </summary>
	public class ConfigurationDataSetLogging
	{
		// The following comment tells ULS auto-tagging to not tag below
		// ASSERTTAG_IGNORE_START

		/// <summary>
		/// Report an exception
		/// </summary>
		/// <param name="tagid">tag</param>
		/// <param name="category">category</param>
		/// <param name="exception">exception</param>
		/// <param name="addToLoadingErrors">add to dataset loading errors</param>
		/// <param name="message">message</param>
		/// <param name="parameters">message format parameters</param>
		public void ReportExceptionTag(uint tagid, Category category, Exception exception, bool addToLoadingErrors, string message,
			params object[] parameters)
		{
			ULSLogging.ReportExceptionTag(tagid, category, exception, message, parameters);

			if (addToLoadingErrors)
			{
				string fullError = string.Format(CultureInfo.InvariantCulture, "{0} Exception: {1}", message, exception);
				LogError(fullError, parameters);
			}
		}


		/// <summary>
		/// Log trace tag. This raises a LogEvent of a specified level.
		/// </summary>
		/// <param name="tagid">tag</param>
		/// <param name="category">category</param>
		/// <param name="level">level to log at</param>
		/// <param name="addToLoadingErrors">add to dataset loading errors</param>
		/// <param name="message">message</param>
		/// <param name="parameters">message format parameters</param>
		public void LogTraceTag(uint tagid, Category category, Level level, bool addToLoadingErrors, string message,
			params object[] parameters)
		{
			ULSLogging.LogTraceTag(tagid, category, level, message, parameters);

			if (addToLoadingErrors)
			{
				LogError(message, parameters);
			}
		}


		/// <summary>
		/// Log a code error. This raises a ShipAssert event
		/// </summary>
		/// <param name="tag">tag</param>
		/// <param name="category">category</param>
		/// <param name="skipLog">should the logging be skipped</param>
		/// <param name="addToLoadingErrors">add to dataset loading errors</param>
		/// <param name="message">message</param>
		/// <param name="parameters">message format parameters</param>
		public void LogCodeErrorTag(uint tag, Category category, bool skipLog, bool addToLoadingErrors, string message, params object[] parameters)
		{
			ULSLogging.LogTraceTag(tag, category, Levels.Error, message, parameters);

			if (addToLoadingErrors)
			{
				LogError(message, parameters);
			}
		}


		/// <summary>
		/// Log a verbose message for debugging / informational purposes. This raises a
		/// LogEvent of level 'Verbose'
		/// </summary>
		/// <param name="tagid">tag</param>
		/// <param name="category">category</param>
		/// <param name="message">message</param>
		/// <param name="parameters">message format parameters</param>
		public void LogVerboseTag(uint tagid, Category category, string message,
			params object[] parameters)
		{
			ULSLogging.LogTraceTag(tagid, category, Levels.Verbose, message, parameters);
		}

		// ASSERTTAG_IGNORE_FINISH
		// The preceding comment tells ULS auto-tagging to not tag above

		/// <summary>
		/// Loading errors
		/// </summary>
		public IList<string> Errors { get; private set; }


		/// <summary>
		/// Log a data set loading error
		/// </summary>
		/// <param name="error">Error to log</param>
		/// <param name="args">Formatting arguments</param>
		/// <returns>Formatted error message</returns>
		protected string LogError(string error, params object[] args)
		{
			if (Errors == null)
			{
				Errors = new List<string>();
			}

			string errorMessage = string.Format(CultureInfo.InvariantCulture, error, args);
			Errors.Add(errorMessage);
			return errorMessage;
		}
	}
}