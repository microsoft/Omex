/***************************************************************************
	ReportExceptionEventArgs.cs

	Event arguments for a report exception event
***************************************************************************/

using System;

namespace Microsoft.Omex.System.Logging
{
	/// <summary>
	/// Event arguments for a report exception event
	/// </summary>
	public class ReportExceptionEventArgs : LogEventArgs
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="tagid">tagid (uniqueid) of the trace</param>
		/// <param name="category">logging category</param>
		/// <param name="exception">exception to log</param>
		/// <param name="message">message to log</param>
		/// <param name="parameters">additional parameters</param>
		public ReportExceptionEventArgs(uint tagid, Category category, Exception exception, string message, params object[] parameters)
			: base(tagid, category, Levels.Error, message, parameters)
		{
			Exception = exception;
		}


		/// <summary>
		/// Exception to log
		/// </summary>
		public Exception Exception { get; }
	}
}
