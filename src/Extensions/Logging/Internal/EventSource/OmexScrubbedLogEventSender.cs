// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.Omex.Extensions.Logging.Scrubbing;

namespace Microsoft.Omex.Extensions.Logging
{
	internal sealed class OmexScrubbedLogEventSender : OmexLogEventSender
	{
		public OmexScrubbedLogEventSender(OmexLogEventSource eventSource, IExecutionContext executionContext, IServiceContext context, IOptionsMonitor<OmexLoggingOptions> options) :
			base(eventSource, executionContext, context, options)
		{
		}

		public new void LogMessage(Activity? activity, string category, LogLevel level, EventId eventId, int threadId, string message, Exception? exception) =>
			base.LogMessage(activity, category, level, eventId, threadId, LogScrubber.Instance.Scrub(message), exception);
	}
}
