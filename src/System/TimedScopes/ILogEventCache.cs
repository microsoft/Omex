// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Omex.System.Logging;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Interface for a log event cache with log replay logic.
	/// </summary>
	public interface ILogEventCache
	{
		/// <summary>
		/// Next process-wide sequence number for event caching
		/// </summary>
		long NextSequenceNumber { get; }

		/// <summary>
		/// Add a log event to the cache and replay if required.
		/// </summary>
		/// <param name="logEventArgs">Log event</param>
		void AddLogEvent(LogEventArgs logEventArgs);

		/// <summary>
		/// Query the events for a specified correlation
		/// </summary>
		/// <param name="correlation">Target correlation Id</param>
		/// <returns>list of trace events</returns>
		IReadOnlyList<LogEventArgs> GetEventsForCorrelation(Guid correlation);

		/// <summary>
		/// Query the trace for a specified correlation
		/// </summary>
		/// <param name="correlation">Target correlation Id</param>
		/// <returns>list of trace events</returns>
		IReadOnlyList<LogEntry> GetTraceForCorrelation(Guid correlation);

		/// <summary>
		/// Query the trace for entries starting at a specified sequence number
		/// </summary>
		/// <param name="sequenceNumber">Start number</param>
		/// <returns>enumeration of trace events</returns>
		IEnumerable<LogEntry> GetTraceFromSequenceNumber(long sequenceNumber);

		/// <summary>
		/// Replay a specified correlation
		/// </summary>
		/// <param name="correlation">Target correlation Id</param>
		void ReplayCorrelation(Guid correlation);
	}
}
