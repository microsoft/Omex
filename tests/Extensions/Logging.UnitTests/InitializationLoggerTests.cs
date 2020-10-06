// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.Omex.Extensions.Abstractions.EventSources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	public class InitializationLoggerTests
	{
		[DataTestMethod]
		[DataRow(EventLevel.Informational)]
		public void LogStatic_CreatesProperEvents(EventLevel eventLevel)
		{
			CustomEventListener listener = new CustomEventListener();
			listener.EnableEvents(ServiceInitializationEventSource.Instance, eventLevel);

			string message = "Test message";
			string category = "Test category";
			Activity activity = new Activity("Test activity");
			activity.Start().Stop(); // start and stop activity to get correlation id

			InitializationLogger.InitilizationSucceed(category, message);

			EventWrittenEventArgs eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)EventSourcesEventIds.GenericHostBuildSucceeded);

			AssertPayload(eventInfo, "message", "Initilization successful for Test category, Test message");

			string newMessage = "New message";
			InitializationLogger.InitilizationFail(category, new Exception("Not expected to be part of the event"), newMessage);

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
