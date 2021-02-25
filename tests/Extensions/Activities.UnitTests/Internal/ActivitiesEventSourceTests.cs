// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.EventSources;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.Omex.Extensions.Activities;
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
			TestEventListener listener = new TestEventListener();
			listener.EnableEvents(ActivityEventSource.Instance, EventLevel.Informational);

			string name = "TestName";
			string subType = "TestSubType";
			string metaData = "TestmetaData";
			Mock<IExecutionContext> contextMock = new Mock<IExecutionContext>();
			contextMock.Setup(c => c.ServiceName).Returns("TestService");

			ActivityEventSender logEventSource = new ActivityEventSender(
				ActivityEventSource.Instance,
				contextMock.Object,
				new NullLogger<ActivityEventSender>());

			string expectedActivityId = string.Empty;
			Guid correlationId = Guid.NewGuid();
			Activity activity = new Activity(name).Start();
			using (activity)
			{
				expectedActivityId = activity.Id ?? string.Empty;
				activity.SetSubType(subType);
				activity.SetMetadata(metaData);
				activity.SetUserHash("TestUserHash");
#pragma warning disable CS0618 // Type or member is obsolete
				activity.SetObsoleteCorrelationId(correlationId);
#pragma warning restore CS0618 // Type or member is obsolete
				if (isHealthCheck)
				{
					activity.MarkAsHealthCheck();
				}
			}

			logEventSource.SendActivityMetric(activity);

			EventWrittenEventArgs eventInfo = listener.EventsInformation.Single(e => e.EventId == (int)eventId);

			eventInfo.AssertPayload("name", name);
			eventInfo.AssertPayload("subType", subType);
			eventInfo.AssertPayload("metadata", metaData);
			eventInfo.AssertPayload("activityId", expectedActivityId);
			eventInfo.AssertPayload("correlationId", correlationId.ToString());
		}
	}
}
