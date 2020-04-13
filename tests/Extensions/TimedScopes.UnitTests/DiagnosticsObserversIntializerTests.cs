// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.TimedScopes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hosting.Services.UnitTests
{
	[TestClass]
	public class DiagnosticsObserversInitializerTests
	{
		[TestMethod]
		public void ExtractExceptionFromPayload_HandleNullValue()
		{
			Exception? result = DiagnosticsObserversIntializer.ExtractExceptionFromPayload(null);
			Assert.IsNull(result);
		}

		[TestMethod]
		public void ExtractExceptionFromPayload_HandleExceptionPayload()
		{
			NullReferenceException exception = new NullReferenceException();
			Exception? result = DiagnosticsObserversIntializer.ExtractExceptionFromPayload(exception);
			Assert.AreEqual(exception, result);
		}

		[TestMethod]
		public void ExtractExceptionFromPayload_HandleExceptionProperty()
		{
			ArgumentException exception = new ArgumentException();
			Exception? result = DiagnosticsObserversIntializer.ExtractExceptionFromPayload(new { Exception = exception });
			Assert.AreEqual(exception, result);
		}

		[TestMethod]
		public async Task DiagnosticsObserversInitializer_InvokedProperly()
		{
			string name = nameof(DiagnosticsObserversInitializer_InvokedProperly);
			MockLogger logger = new MockLogger();
			Mock<IActivityStartObserver> startObserver = new Mock<IActivityStartObserver>();
			Mock<IActivityStopObserver> stopObserver = new Mock<IActivityStopObserver>();
			DiagnosticsObserversInitializer initializer = new DiagnosticsObserversInitializer(
				new[] { startObserver.Object },
				new[] { stopObserver.Object },
				logger);

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

				Exception exception = new ArithmeticException();
				listener.Write(ExceptionEventName, exception);
				Assert.IsTrue(logger.Exceptions.Contains(exception), "Should log exception event");
			}
			catch
			{
				await initializer.StopAsync(CancellationToken.None).ConfigureAwait(false);
				throw;
			}
		}

		private void AssertEnabledFor(DiagnosticListener listener, string eventName) =>
			Assert.IsTrue(listener.IsEnabled(eventName), "Should be enabled for '{0}'", eventName);

		private string MakeStartName(string name) => name + DiagnosticsObserversIntializer.ActivityStartEnding;

		private string MakeStopName(string name) => name + DiagnosticsObserversIntializer.ActivityStopEnding;

		private const string ExceptionEventName = "System.Net.Http.Exception";

		private const string HttpRequestOutEventName = "System.Net.Http.HttpRequestOut";

		private const string HttpRequestInEventName = "Microsoft.AspNetCore.Hosting.HttpRequestIn";

		private class MockLogger : ILogger<DiagnosticsObserversIntializer>
		{
			public List<Exception> Exceptions { get; } = new List<Exception>();

			public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();

			public bool IsEnabled(LogLevel logLevel) => true;

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) => Exceptions.Add(exception);
		}
	}
}
