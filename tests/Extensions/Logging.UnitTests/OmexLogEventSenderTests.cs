// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.EventSources;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.Omex.Extensions.Testing.Helpers;
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
			TestEventListener listener = new TestEventListener();
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

			eventInfo.AssertPayload("message", message);
			eventInfo.AssertPayload("category", category);
			eventInfo.AssertPayload("activityId", activity.Id ?? string.Empty);
			eventInfo.AssertPayload("tagId", "fff9");

			InitializationLogger.LogInitializationSucceed(category, message);


			eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)EventSourcesEventIds.GenericHostBuildSucceeded);

			eventInfo.AssertPayload("message", "Initialization successful for Test category, Test message");

			string newMessage = "New message";
			InitializationLogger.LogInitializationFail(category, new Exception("Not expected to be part of the event"), newMessage);

			eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)EventSourcesEventIds.GenericHostFailed);
			eventInfo.AssertPayload("message", newMessage);
		}
	}
}
