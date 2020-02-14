// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Logging.Replayable
{
	internal struct LogMessageInformation
	{
		public string Category { get; }
		public EventId EventId { get; }
		public int ThreadId { get; }
		public string Message { get; }


		public LogMessageInformation(string category, EventId eventId, int threadId, string message)
		{
			Category = category;
			EventId = eventId;
			ThreadId = threadId;
			Message = message;
		}
	}
}
