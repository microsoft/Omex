// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
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
		[DataRow(EventLevel.Error, LogLevel.Critical, EventSourcesEventIds.LogErrorEventId)]
		[DataRow(EventLevel.Error, LogLevel.Error, EventSourcesEventIds.LogErrorEventId)]
		[DataRow(EventLevel.Warning, LogLevel.Warning, EventSourcesEventIds.LogWarningEventId)]
		[DataRow(EventLevel.Informational, LogLevel.Information, EventSourcesEventIds.LogInfoEventId)]
		[DataRow(EventLevel.Verbose, LogLevel.Debug, EventSourcesEventIds.LogVerboseEventId)]
		[DataRow(EventLevel.Verbose, LogLevel.Trace, EventSourcesEventIds.LogSpamEventId)]
		public void EventSourceLogsMessage(EventLevel eventLevel, LogLevel logLevel, EventSourcesEventIds eventId)
		{
			CustomEventListener listener = new CustomEventListener();
			listener.EnableEvents(s_logEvent, eventLevel);

			string message = "Test message";
			string category = "Test category";
			string activityId = "Test activity";
			int tagId = 0xFFF9;
			s_logEvent.LogMessage(activityId, default, category, logLevel, tagId, 0, message);

			EventWrittenEventArgs eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)eventId);

			AssertPayload(eventInfo, "message", message);
			AssertPayload(eventInfo, "category", category);
			AssertPayload(eventInfo, "correlationId", activityId);
			AssertPayload(eventInfo, "tagId", "fff9");
		}


		private void AssertPayload<T>(EventWrittenEventArgs info, string name, T expected)
			where T : class
		{
			int index = info.PayloadNames?.IndexOf(name) ?? -1;

			T? value = (T?)(index < 0 ? null : info.Payload?[index]);

			Assert.AreEqual(expected, value, $"Wrong value for {name}");
		}


		private class CustomEventListener : EventListener
		{
			public List<EventWrittenEventArgs> EventsInformation { get; } = new List<EventWrittenEventArgs>();

			protected override void OnEventWritten(EventWrittenEventArgs eventData) => EventsInformation.Add(eventData);
		}


		private static readonly OmexLogsEventSource s_logEvent = new OmexLogsEventSource(new BasicMachineInformation(), new EmptyServiceContext());
	}
}
