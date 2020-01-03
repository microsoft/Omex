// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>Information about log message</summary>
	public struct LogMessageInformation
	{
		/// <summary>Category name</summary>
		public string Category { get; }

		/// <summary>Trace Id</summary>
		public string TraceId { get; }

		/// <summary>Event Id</summary>
		public EventId EventId { get; }

		/// <summary>Thread Id</summary>
		public int ThreadId { get; }

		/// <summary>Log message</summary>
		public string Message { get; }


		/// <summary>Create log message information structure</summary>
		/// <param name="category">Name of the category</param>
		/// <param name="traceId">Trace Id</param>
		/// <param name="eventId">Event Id</param>
		/// <param name="threadId">thread Id</param>
		/// <param name="message">Log message</param>
		public LogMessageInformation(string category, string traceId, EventId eventId, int threadId, string message)
		{
			Category = category;
			TraceId = traceId;
			EventId = eventId;
			ThreadId = threadId;
			Message = message;
		}
	}
}
