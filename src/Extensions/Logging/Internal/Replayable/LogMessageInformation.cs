// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Logging.Replayable
{
	internal readonly struct LogMessageInformation
	{
		public string Category { get; }
		public EventId EventId { get; }
		public int ThreadId { get; }
		public string Message { get; }
		public Exception? Exception { get; }

		public LogMessageInformation(string category, EventId eventId, int threadId, string message, Exception? exception)
		{
			Category = category;
			EventId = eventId;
			ThreadId = threadId;
			Message = message;
			Exception = exception;
		}
	}
}
