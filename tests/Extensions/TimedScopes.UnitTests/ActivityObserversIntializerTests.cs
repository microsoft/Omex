// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.TimedScopes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hosting.Services.UnitTests
{
	[TestClass]
	public class ActivityObserversIntializerTests
	{
		[TestMethod]
		public async Task ActivityObserversInvokedProperly()
		{
			string name = nameof(ActivityObserversInvokedProperly);
			Mock<ILogger<ActivityObserversIntializer>> loggerMock = new Mock<ILogger<ActivityObserversIntializer>>();
			Mock<IActivityStartObserver> startObserver = new Mock<IActivityStartObserver>();
			Mock<IActivityStopObserver> stopObserver = new Mock<IActivityStopObserver>();
			ActivityObserversIntializer initializer = new ActivityObserversIntializer(
				new [] { startObserver.Object },
				new [] { stopObserver.Object },
				loggerMock.Object);

			try
			{
				await initializer.StartAsync(CancellationToken.None).ConfigureAwait(false);

				using DiagnosticListener listener = new DiagnosticListener(name);

				AssertEnabledFor(listener, HttpRequestOutEventName);
				AssertEnabledFor(listener, HttpRequestInEventName);
				AssertEnabledFor(listener, ExceptionEventName);

				AssertEnabledFor(listener, MakeStartName(name));
				AssertEnabledFor(listener, MakeStopName(name));

				Assert.IsFalse(listener.IsEnabled(name, "Should be disabled for other events"));

				Activity activity = new Activity(name);
				object obj = new object();

				listener.StartActivity(activity, obj);
				startObserver.Verify(obs => obs.OnStart(activity, obj), Times.Once);

				listener.StopActivity(activity, obj);
				stopObserver.Verify(obs => obs.OnStop(activity, obj), Times.Once);

				Assert.IsFalse(loggerMock.Invocations.Any(), "Logger invocation should be empty before");
				listener.Write(ExceptionEventName, new ArithmeticException());
				Assert.IsTrue(loggerMock.Invocations.Count == 1, "Should log exception event");
			}
			catch
			{
				await initializer.StopAsync(CancellationToken.None).ConfigureAwait(false);
				throw;
			}
		}

		private void AssertEnabledFor(DiagnosticListener listener, string eventName) =>
			Assert.IsTrue(listener.IsEnabled(eventName), "Should be enabled for '{0}'", eventName);

		private string MakeStartName(string name) => name + ActivityObserversIntializer.ActivityStartEnding;

		private string MakeStopName(string name) => name + ActivityObserversIntializer.ActivityStopEnding;

		private const string ExceptionEventName = "System.Net.Http.Exception";

		private const string HttpRequestOutEventName = "System.Net.Http.HttpRequestOut";

		private const string HttpRequestInEventName = "Microsoft.AspNetCore.Hosting.HttpRequestIn";
	}
}
