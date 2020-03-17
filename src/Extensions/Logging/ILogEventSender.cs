// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Event source to send log messages
	/// </summary>
	public interface ILogEventSender
	{
		/// <summary>
		/// Is logging enabled for specified log level
		/// </summary>
		bool IsEnabled(LogLevel level);


		/// <summary>
		/// Should this message be replayed in case of failure
		/// </summary>
		bool IsReplayableMessage(LogLevel logLevel);


		/// <summary>
		/// Log message
		/// </summary>
		/// <param name="activity">Activity for this log event</param>
		/// <param name="category">Log category</param>
		/// <param name="level">Log level</param>
		/// <param name="eventId">event Id</param>
		/// <param name="threadId">Id of the thread</param>
		/// <param name="message">Log message</param>
		void LogMessage(Activity activity, string category, LogLevel level, EventId eventId, int threadId, string message);
	}
}
