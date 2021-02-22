// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.Activities;
using Microsoft.Omex.Extensions.Activities.UnitTests;
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
			Exception? result = DiagnosticsObserversInitializer.ExtractExceptionFromPayload(null);
			Assert.IsNull(result);
		}

		[TestMethod]
		public void ExtractExceptionFromPayload_HandleExceptionPayload()
		{
			NullReferenceException exception = new NullReferenceException();
			Exception? result = DiagnosticsObserversInitializer.ExtractExceptionFromPayload(exception);
			Assert.AreEqual(exception, result);
		}

		[TestMethod]
		public void ExtractExceptionFromPayload_HandleExceptionProperty()
		{
			ArgumentException exception = new ArgumentException();
			Exception? result = DiagnosticsObserversInitializer.ExtractExceptionFromPayload(new { Exception = exception });
			Assert.AreEqual(exception, result);
		}

		[TestMethod]
		public async Task DiagnosticsObserversInitializer_InvokedProperly()
		{
			string name = nameof(DiagnosticsObserversInitializer_InvokedProperly);
			MockLogger logger = new();
			DiagnosticsObserversInitializer diagnosticsInitializer = new(logger);

			try
			{
				await diagnosticsInitializer.StartAsync(CancellationToken.None).ConfigureAwait(false);

				using DiagnosticListener listener = new DiagnosticListener(name);

				AssertEnabledFor(listener, HttpClientListenerName);
				AssertEnabledFor(listener, HttpRequestOutEventName);
				AssertEnabledFor(listener, HttpRequestInEventName);
				AssertEnabledFor(listener, ExceptionEventName);

				Assert.IsFalse(listener.IsEnabled(name, "Should be disabled for other events"));

				Exception exception = new ArithmeticException();
				listener.Write(ExceptionEventName, exception);
				Assert.IsTrue(logger.Exceptions.Contains(exception), "Should log exception event");
			}
			catch
			{
				await diagnosticsInitializer.StopAsync(CancellationToken.None).ConfigureAwait(false);
				throw;
			}
		}

		private void AssertEnabledFor(DiagnosticListener listener, string eventName) =>
			Assert.IsTrue(listener.IsEnabled(eventName), "Should be enabled for '{0}'", eventName);

		private const string ExceptionEventName = "System.Net.Http.Exception";

		private const string HttpClientListenerName = "HttpHandlerDiagnosticListener";

		private const string HttpRequestOutEventName = "System.Net.Http.HttpRequestOut";

		private const string HttpRequestInEventName = "Microsoft.AspNetCore.Hosting.HttpRequestIn";

		private class MockLogger : ILogger<DiagnosticsObserversInitializer>
		{
			public List<Exception> Exceptions { get; } = new();

			public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();

			public bool IsEnabled(LogLevel logLevel) => true;

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) => Exceptions.Add(exception);
		}
	}
}
