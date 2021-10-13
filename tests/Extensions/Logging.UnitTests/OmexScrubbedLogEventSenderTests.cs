// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.EventSources;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.Omex.Extensions.Logging.Scrubbing;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	public class OmexScrubbedLogEventSenderTests
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
			using TestEventListener listener = new();
			listener.EnableEvents(OmexLogEventSource.Instance, eventLevel);
			listener.EnableEvents(ServiceInitializationEventSource.Instance, EventLevel.Informational);

			const string message = "Test message";
			const string category = "Test category";
			const int tagId = 0xFFF9;
			string expectedActivityId;
			using (Activity activity = new("Test activity"))
			{
				activity.Start().Stop(); // Start and stop the activity to get the correlation ID.
				expectedActivityId = activity.Id!;

				Mock<IOptionsMonitor<OmexLoggingOptions>> mockOptions = new();
				mockOptions.Setup(m => m.CurrentValue).Returns(new OmexLoggingOptions());

				LogScrubber.Instance.ClearRules();

				OmexScrubbedLogEventSender logsSender = new(
					OmexLogEventSource.Instance,
					new Mock<IExecutionContext>().Object,
					new EmptyServiceContext(),
					mockOptions.Object);

				logsSender.LogMessage(activity, category, logLevel, tagId, 0, message, new Exception("Not expected to be part of the event"));
			}

			EventWrittenEventArgs eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)eventId);
			eventInfo.AssertPayload("message", message);
			eventInfo.AssertPayload("category", category);
			eventInfo.AssertPayload("activityId", expectedActivityId);
			eventInfo.AssertPayload("tagId", tagId.ToString("x4"));

			InitializationLogger.LogInitializationSucceed(category, message);
			eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)EventSourcesEventIds.GenericHostBuildSucceeded);
			eventInfo.AssertPayload("message", "Initialization successful for Test category, Test message");

			const string newMessage = "New message";
			InitializationLogger.LogInitializationFail(category, new Exception("Not expected to be part of the event"), newMessage);
			eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)EventSourcesEventIds.GenericHostFailed);
			eventInfo.AssertPayload("message", newMessage);
		}

		[DataTestMethod]
		[DataRow(EventLevel.Error, LogLevel.Critical, EventSourcesEventIds.LogError)]
		[DataRow(EventLevel.Error, LogLevel.Error, EventSourcesEventIds.LogError)]
		[DataRow(EventLevel.Warning, LogLevel.Warning, EventSourcesEventIds.LogWarning)]
		[DataRow(EventLevel.Informational, LogLevel.Information, EventSourcesEventIds.LogInfo)]
		[DataRow(EventLevel.Verbose, LogLevel.Debug, EventSourcesEventIds.LogVerbose)]
		[DataRow(EventLevel.Verbose, LogLevel.Trace, EventSourcesEventIds.LogSpam)]
		public void LogMessage_Scrubs(EventLevel eventLevel, LogLevel logLevel, EventSourcesEventIds eventId)
		{
			using TestEventListener listener = new();
			listener.EnableEvents(OmexLogEventSource.Instance, eventLevel);
			listener.EnableEvents(ServiceInitializationEventSource.Instance, EventLevel.Informational);

			const string message = "Test message";
			const string category = "Test category";
			const int tagId = 0xFFF9;
			string expectedActivityId;
			using (Activity activity = new("Test activity"))
			{
				activity.Start().Stop(); // Start and stop the activity to get the correlation ID.
				expectedActivityId = activity.Id!;

				Mock<IOptionsMonitor<OmexLoggingOptions>> mockOptions = new();
				mockOptions.Setup(m => m.CurrentValue).Returns(new OmexLoggingOptions());
				
				LogScrubber.Instance.ClearRules();
				LogScrubber.Instance.AddRule(new ScrubberRule(new Regex("Test"), "[REDACTED]"));

				OmexScrubbedLogEventSender logsSender = new(
					OmexLogEventSource.Instance,
					new Mock<IExecutionContext>().Object,
					new EmptyServiceContext(),
					mockOptions.Object);

				logsSender.LogMessage(activity, category, logLevel, tagId, 0, message, new Exception("Not expected to be part of the event"));
			}

			EventWrittenEventArgs eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)eventId);
			eventInfo.AssertPayload("message", "[REDACTED] message");
			eventInfo.AssertPayload("category", category);
			eventInfo.AssertPayload("activityId", expectedActivityId);
			eventInfo.AssertPayload("tagId", tagId.ToString("x4"));
		}
	}
}
