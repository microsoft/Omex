// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	public class OmexLogsEventSourceTests
	{
		[DataTestMethod]
		[DataRow(EventLevel.Error, LogLevel.Error, EventSourcesEventIds.LogErrorEventId)]
		[DataRow(EventLevel.Warning, LogLevel.Warning, EventSourcesEventIds.LogWarningEventId)]
		[DataRow(EventLevel.Informational, LogLevel.Information, EventSourcesEventIds.LogInfoEventId)]
		public void EventSourceLogsMessage(EventLevel eventLevel, LogLevel logLevel, EventSourcesEventIds eventId)
		{
			

			CustomEventListener listener = new CustomEventListener();
			listener.EnableEvents(s_logEvent, eventLevel);

			int tagId = 0x4FFFFFFF;
			s_logEvent.LogMessage("0", default, "0", logLevel, tagId, 0, "My message ");

			EventWrittenEventArgs eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)eventId);
			//TODO check that event agrs are correct
		}


		private class CustomEventListener : EventListener
		{
			public List<EventWrittenEventArgs> EventsInformation { get; } = new List<EventWrittenEventArgs>();

			protected override void OnEventWritten(EventWrittenEventArgs eventData) => EventsInformation.Add(eventData);
		}


		private static readonly OmexLogsEventSource s_logEvent = new OmexLogsEventSource(new BasicMachineInformation(), new EmptyServiceContext());
	}
}
