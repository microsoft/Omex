/***************************************************************************
	ULSLogging.cs

	Unified logging, allows registering for logging events sent by
	code implementations and to raise logging events
***************************************************************************/

using System;

namespace Microsoft.Omex.System.Logging
{
	/// <summary>
	/// Unified logging, allows registering for logging events sent by
	/// code implementations and to raise logging events
	/// </summary>
	public static class ULSLogging
	{
		/// <summary>
		/// Log a trace
		/// </summary>
		/// <param name="tagid">tagid (uniqueid) of the trace</param>
		/// <param name="category">logging category</param>
		/// <param name="level">logging level</param>
		/// <param name="message">message to log</param>
		/// <param name="parameters">additional parameters</param>
		public static void LogTraceTag(uint tagid, Category category, Level level, string message, params object[] parameters)
		{
			LogEvent?.Invoke(LogEventSender, new LogEventArgs(tagid, category, level, message, parameters));
		}


		/// <summary>
		/// Event handler for log events
		/// </summary>
		public static event EventHandler<LogEventArgs> LogEvent;


		/// <summary>
		/// Raise a log event
		/// </summary>
		/// <param name="sender">sender of event</param>
		/// <param name="e">event arguments</param>
		public static void RaiseLogEvent(object sender, LogEventArgs e)
		{
			if (e != null)
			{
				LogEvent?.Invoke(sender ?? LogEventSender, e);
			}
		}


		/// <summary>
		/// Report an exception
		/// </summary>
		/// <param name="tagid">tag</param>
		/// <param name="category">category</param>
		/// <param name="exception">exception</param>
		/// <param name="message">message</param>
		/// <param name="parameters">message format parameters</param>
		public static void ReportExceptionTag(uint tagid, Category category, Exception exception, string message,
			params object[] parameters)
		{
			LogEvent?.Invoke(LogEventSender, new ReportExceptionEventArgs(tagid, category, exception, message, parameters));
		}


		/// <summary>
		/// Optional log event sender, added as sender of event
		/// </summary>
		public static object LogEventSender { get; set; }
	}
}
