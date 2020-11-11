// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.EventSources;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	public class OmexLogEventSenderTests
	{
		[DataTestMethod]
		[DataRow(EventLevel.Error, LogLevel.Critical, EventSourcesEventIds.LogError)]
		[DataRow(EventLevel.Error, LogLevel.Error, EventSourcesEventIds.LogError)]
		[DataRow(EventLevel.Warning, LogLevel.Warning, EventSourcesEventIds.LogWarning)]
		[DataRow(EventLevel.Informational, LogLevel.Information, EventSourcesEventIds.LogInfo)]
		[DataRow(EventLevel.Verbose, LogLevel.Debug, EventSourcesEventIds.LogVerbose)]
		[DataRow(EventLevel.Verbose, LogLevel.Trace, EventSourcesEventIds.LogSpam)]
		public void LogMessage_CreatesProperEvents(EventLevel eventLevel, LogLevel logLevel, EventSourcesEventIds eventId)
		{
			CustomEventListener listener = new CustomEventListener();
			listener.EnableEvents(OmexLogEventSource.Instance, eventLevel);
			listener.EnableEvents(ServiceInitializationEventSource.Instance, EventLevel.Informational);

			string message = "Test message";
			string category = "Test category";
			int tagId = 0xFFF9;
			Activity activity = new Activity("Test activity");
			activity.Start().Stop(); // start and stop activity to get correlation id

			OmexLogEventSender logsSender = new OmexLogEventSender(
				OmexLogEventSource.Instance,
				new Mock<IExecutionContext>().Object,
				new EmptyServiceContext(),
				Options.Create(new OmexLoggingOptions()));

			logsSender.LogMessage(activity, category, logLevel, tagId, 0, message, new Exception("Not expected to be part of the event"));

			EventWrittenEventArgs eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)eventId);

			AssertPayload(eventInfo, "message", message);
			AssertPayload(eventInfo, "category", category);
			AssertPayload(eventInfo, "activityId", activity.Id ?? string.Empty);
			AssertPayload(eventInfo, "tagId", "fff9");

			InitializationLogger.LogInitializationSucceed(category, message);


			eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)EventSourcesEventIds.GenericHostBuildSucceeded);

			AssertPayload(eventInfo, "message", "Initilization successful for Test category, Test message");

			string newMessage = "New message";
			InitializationLogger.LogInitializationFail(category, new Exception("Not expected to be part of the event"), newMessage);

			eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)EventSourcesEventIds.GenericHostFailed);
			AssertPayload(eventInfo, "message", newMessage);
		}

		private void AssertPayload<TPayloadType>(EventWrittenEventArgs info, string name, TPayloadType expected)
			where TPayloadType : class
		{
			int index = info.PayloadNames?.IndexOf(name) ?? -1;

			TPayloadType? value = (TPayloadType?)(index < 0 ? null : info.Payload?[index]);

			Assert.AreEqual(expected, value, $"Wrong value for {name}");
		}

		private class CustomEventListener : EventListener
		{
			public List<EventWrittenEventArgs> EventsInformation { get; } = new List<EventWrittenEventArgs>();

			protected override void OnEventWritten(EventWrittenEventArgs eventData) => EventsInformation.Add(eventData);
		}
	}
}
