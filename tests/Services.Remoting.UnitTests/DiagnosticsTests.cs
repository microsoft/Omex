// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Omex.Extensions.Services.Remoting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Services.Remoting
{

	[TestClass]
	public class DiagnosticsTests
	{
		[TestMethod]
		public void ReportException_PropagatedAsEventPayload()
		{
			(DiagnosticListener listener, MockObserver mockObserver) = MockObserver.CreateListener();
			Exception exception = new IndexOutOfRangeException();

			listener.ReportException(exception);

			mockObserver.AssertException(exception);
		}

		[TestMethod]
		public void CreateAndStartActivity_ReportsStartEvent()
		{
			(DiagnosticListener listener, MockObserver mockObserver) = MockObserver.CreateListener();

			string activityName = nameof(CreateAndStartActivity_ReportsStartEvent);
			Activity? activity = listener.CreateAndStartActivity(activityName);
			string eventName = mockObserver.Events.Single().Key;

			Assert.AreEqual(activityName + ".Start", eventName);
		}

		[TestMethod]
		public void CreateAndStartActivity_DontCreateEventsIfNobodyListens()
		{
			string name = nameof(CreateAndStartActivity_DontCreateEventsIfNobodyListens);
			DiagnosticListener listener = new(name);

			Activity? activity = listener.CreateAndStartActivity(name);

			Assert.IsNull(activity);
		}

		[TestMethod]
		public void StopActivityIfExist_ReportsStopEvent()
		{
			(DiagnosticListener listener, MockObserver mockObserver) = MockObserver.CreateListener();

			string activityName = nameof(StopActivityIfExist_ReportsStopEvent);
			Activity activity = new Activity(activityName).Start();

			listener.StopActivityIfExist(activity);
			string eventName = mockObserver.Events.Single().Key;

			Assert.AreEqual(activityName + ".Stop", eventName);
			Assert.AreNotEqual(TimeSpan.Zero, activity.Duration, "Activity should be stoped");
		}

		[TestMethod]
		public void StopActivityIfExist_HandlesNullValue()
		{
			(DiagnosticListener listener, MockObserver mockObserver) = MockObserver.CreateListener();

			listener.StopActivityIfExist(null);
			Assert.IsFalse(mockObserver.Events.Any(), "Should not produce any events");
		}

		[TestMethod]
		public void DiagnosticListenerName_CorrespondsAssemblyName()
		{
			Assert.AreEqual(Diagnostics.DiagnosticListenerName, typeof(Diagnostics).Assembly.GetName().Name, "DiagnosticListenerName should be the same as assembly name");
		}
	}
}
