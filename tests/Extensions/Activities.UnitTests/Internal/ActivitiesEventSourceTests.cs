// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.EventSources;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Activities.UnitTests
{
	[TestClass]
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

			ActivityEventSender logEventSource = new(
				ActivityEventSource.Instance,
				contextMock.Object,
				new NullLogger<ActivityEventSender>());

			string? expectedActivityId;
			Guid correlationId = Guid.NewGuid();
			using (Activity activity = new Activity(name).Start())
			{
				expectedActivityId = activity.Id;
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
			}

			EventWrittenEventArgs eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)eventId);
			eventInfo.AssertPayload("name", name);
			eventInfo.AssertPayload("subType", subType);
			eventInfo.AssertPayload("metadata", metadata);
			eventInfo.AssertPayload("activityId", expectedActivityId!);
			eventInfo.AssertPayload("correlationId", correlationId.ToString());
		}
	}
}
