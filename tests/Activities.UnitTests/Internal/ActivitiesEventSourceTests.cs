// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.EventSources;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Activities.UnitTests
{
	[TestClass]
	[Obsolete($"{nameof(ActivityEventSender)} is obsolete and pending for removal by 1 July 2024.", DiagnosticId = "OMEX188")]
	public class ActivitiesEventSourceTests
	{
		[DataTestMethod]
		[DataRow(EventSourcesEventIds.LogActivityTestContext, true)]
		[DataRow(EventSourcesEventIds.LogActivity, false)]
		public void LogActivityEndEvent_CreatesEvent(EventSourcesEventIds eventId, bool isHealthCheck)
		{
			using TestEventListener listener = new();
			listener.EnableEvents(ActivityEventSource.Instance, EventLevel.Informational);

			const string name = "TestName";
			const string subType = "TestSubType";
			const string metadata = "TestMetadata";

			Mock<IExecutionContext> contextMock = new();
			contextMock.Setup(c => c.ServiceName).Returns("TestService");

			Mock<IOptions<ActivityOption>> mockOptions = new();
			mockOptions.Setup(m => m.Value).Returns(new ActivityOption());

			ActivityEventSender logEventSource = new(
				ActivityEventSource.Instance,
				contextMock.Object,
				new NullLogger<ActivityEventSender>(),
				mockOptions.Object);

			Guid correlationId = Guid.NewGuid();
			using Activity activity = new Activity(name).Start();
			activity.SetSubType(subType);
			activity.SetMetadata(metadata);
			activity.SetUserHash("TestUserHash");
#pragma warning disable CS0618 // Type or member is obsolete
			activity.SetObsoleteCorrelationId(correlationId);
#pragma warning restore CS0618 // Type or member is obsolete
			if (isHealthCheck)
			{
				activity.MarkAsHealthCheck();
			}

			logEventSource.SendActivityMetric(activity);

			EventWrittenEventArgs eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)eventId);
			eventInfo.AssertPayload("name", name);
			eventInfo.AssertPayload("subType", subType);
			eventInfo.AssertPayload("metadata", metadata);
			eventInfo.AssertPayload("activityId", activity.Id);
			eventInfo.AssertPayload("correlationId", correlationId.ToString());
		}

		[DataTestMethod]
		[DataRow(EventSourcesEventIds.LogActivityTestContext, true)]
		[DataRow(EventSourcesEventIds.LogActivity, false)]
		public void LogActivityEndEvent_DisableByOption_CreatesNoEvent(EventSourcesEventIds eventId, bool isHealthCheck)
		{
			using TestEventListener listener = new();
			listener.EnableEvents(ActivityEventSource.Instance, EventLevel.Informational);

			const string name = "TestName";

			Mock<IExecutionContext> contextMock = new();
			contextMock.Setup(c => c.ServiceName).Returns("TestService");

			Mock<IOptions<ActivityOption>> mockOptions = new();
			ActivityOption activityOption = new() { ActivityEventSenderEnabled = false };
			mockOptions.Setup(m => m.Value).Returns(activityOption);

			ActivityEventSender logEventSource = new(
				ActivityEventSource.Instance,
				contextMock.Object,
				new NullLogger<ActivityEventSender>(),
				mockOptions.Object);

			Guid correlationId = Guid.NewGuid();
			using Activity activity = new Activity(name).Start();
			if (isHealthCheck)
			{
				activity.MarkAsHealthCheck();
			}

			logEventSource.SendActivityMetric(activity);

			Assert.AreEqual(0, listener.EventsInformation.Count);
		}
	}
}
