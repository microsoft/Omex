// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hosting.Services.UnitTests
{
	[TestClass]
	public class ActivityObserverTests
	{
		private readonly ObjectPoolProvider m_poolProvider = new DefaultObjectPoolProvider();

		[TestMethod]
		public void OnStop_CallsLogActivityStop()
		{
			using Activity activity = new(nameof(OnStop_CallsLogActivityStop));
			(ActivityObserver observer, _, Mock<IActivitiesEventSender> senderMock) = CreateObserver();

			observer.OnStop(activity, null);

			senderMock.Verify(s => s.SendActivityMetric(activity), Times.Once);
		}

		[TestMethod]
		public void OnStart_LogsStart()
		{
			using Activity parentActivity = new Activity(nameof(OnStart_LogsStart) + "Parent").Start();
			using Activity activity = new Activity(nameof(OnStart_LogsStart)).Start();

			(ActivityObserver observer, TestLogger testLogger, _) = CreateObserver();

			observer.OnStart(activity);

			(EventId id, LogLevel level, string message) = testLogger.Messages.Single();
			StringAssert.Contains(message, activity.Id);
			StringAssert.Contains(message, activity.OperationName);
			StringAssert.Contains(message, activity.ParentId);
		}

		[DataTestMethod]
		[DataRow(true, null)]
		[DataRow(true, ActivityResult.Success)]
		[DataRow(false, ActivityResult.SystemError)]
		[DataRow(false, ActivityResult.ExpectedError)]
		public void OnStop_LogsStop(bool isSuccesful, ActivityResult? result)
		{
			using Activity activity = new Activity(nameof(OnStop_LogsStop))
				.Start()
				.SetBaggage("SomeValue", "BaggageValue")
				.SetTag("SomeTag", "TagValue");

			if (result.HasValue)
			{
				activity.SetResult(result.Value);
			}

			(ActivityObserver observer, TestLogger testLogger, _) = CreateObserver();

			activity.Stop();
			observer.OnStop(activity);

			(EventId id, LogLevel level, string message) = testLogger.Messages.Single();
			StringAssert.Contains(message, activity.Id);
			StringAssert.Contains(message, activity.OperationName);
			StringAssert.Contains(message, activity.Duration.TotalMilliseconds.ToString());

			foreach (KeyValuePair<string, string?> pair in activity.Baggage)
			{
				StringAssert.Contains(message, pair.Key);
				StringAssert.Contains(message, pair.Value);
			}

			foreach (KeyValuePair<string, object?> pair in activity.TagObjects)
			{
				StringAssert.Contains(message, pair.Key);
				StringAssert.Contains(message, pair.Value?.ToString());
			}

			LogLevel expectedLevel = isSuccesful ? LogLevel.Information : LogLevel.Warning;
			Assert.AreEqual(expectedLevel, level);
		}

		private (ActivityObserver observer, TestLogger logger, Mock<IActivitiesEventSender> senderMock) CreateObserver()
		{
			Mock<IActivitiesEventSender> senderMock = new();
			TestLogger testLogger = new();
			ActivityObserver observer = new(senderMock.Object, testLogger, m_poolProvider);
			return (observer, testLogger, senderMock);
		}

		private class TestLogger : ILogger<ActivityObserver>
		{
			public List<(EventId id, LogLevel level, string message)> Messages = new();

			public IDisposable? BeginScope<TState>(TState state) where TState : notnull => throw new NotImplementedException();

			public bool IsEnabled(LogLevel logLevel) => true;

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
				Messages.Add((eventId, logLevel, formatter(state, exception)));
		}
	}
}
