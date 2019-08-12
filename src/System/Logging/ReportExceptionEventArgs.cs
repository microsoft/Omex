// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
			: base(null, false, tagid, category, Levels.Error, message, string.Empty, parameters)
		{
			Exception = exception;
		}


		/// <summary>
		/// Exception to log
		/// </summary>
		public Exception Exception { get; }
	}
}