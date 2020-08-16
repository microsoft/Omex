// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.UnitTests
{
	[TestClass]
	public class HostWrapperWithExceptionLoggingTests
	{
		[TestMethod]
		public async Task HostRun_LogsExceptionsAsCritical()
			=> await AssertExceptionLoggedAsync<ArgumentException>(LogLevel.Critical);

		[TestMethod]
		public async Task HostRun_LogsCancelationAsError()
			=> await AssertExceptionLoggedAsync<OperationCanceledException>(LogLevel.Error);

		private async Task AssertExceptionLoggedAsync<TException>(LogLevel expectedLogLevel)
			where TException : Exception, new()
		{
			TestLoggerProvider provider = new TestLoggerProvider();
			Exception expectedStartException = new TException();
			Exception expectedStopException = new TException();

			IHost host = new HostBuilder()
				.ConfigureServices(service =>
				{
					service
						.AddHostedService(p => new TestHost(expectedStartException, expectedStopException))
						.AddSingleton<ILoggerProvider>(provider);
				})
				.BuildWithErrorReporting();

			Exception startException = await Assert.ThrowsExceptionAsync<TException>(() => host.StartAsync());
			Assert.AreEqual(expectedStartException, startException, nameof(IHost.StartAsync));
			provider.Logger.AssertExceptionLogged(nameof(IHost.StartAsync), startException, expectedLogLevel);

			AggregateException aggregatedException = await Assert.ThrowsExceptionAsync<AggregateException>(() => host.StopAsync());
			Exception stopException = aggregatedException.InnerExceptions.Single();
			Assert.AreEqual(expectedStopException, stopException, nameof(IHost.StopAsync));
			provider.Logger.AssertExceptionLogged(nameof(IHost.StopAsync), stopException, expectedLogLevel);

			Exception disposeException = Assert.ThrowsException<NotImplementedException>(() => host.Dispose());
			Assert.AreEqual(TestHost.DisposeException, disposeException, nameof(IHost.Dispose));
			provider.Logger.AssertExceptionLogged(nameof(IHost.Dispose), disposeException, LogLevel.Critical);
		}

		private class TestHost : IHostedService, IDisposable
		{
			private readonly Exception m_startException;

			private readonly Exception m_stopException;

			public static Exception DisposeException { get; } = new NotImplementedException();

			public TestHost(Exception startException, Exception stopException)
				=> (m_startException, m_stopException) = (startException, stopException);

			public Task StartAsync(CancellationToken cancellationToken) => throw m_startException;

			public Task StopAsync(CancellationToken cancellationToken) => throw m_stopException;

			public void Dispose() => throw DisposeException;
		}

		private class TestLoggerProvider : ILoggerProvider
		{
			public TestLogger Logger { get; } = new TestLogger();

			public ILogger CreateLogger(string categoryName) => Logger;

			public void Dispose() {	}
		}

		private class TestLogger : ILogger
		{
			public Stack<(Exception, LogLevel)> Exceptions { get; } = new Stack<(Exception, LogLevel)>();

			public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();

			public bool IsEnabled(LogLevel logLevel) => true;

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
			{
				if (exception != null)
				{
					Exceptions.Push((exception, logLevel));
				}
			}

			public void AssertExceptionLogged(string message, Exception expectedException, LogLevel expectedLevel)
			{
				(Exception actualException, LogLevel actualLevel) = Exceptions.Pop();
				Assert.AreEqual(expectedException, actualException, message);
				Assert.AreEqual(expectedLevel, actualLevel, message);
				if (Exceptions.Any())
				{
					Assert.Fail("More then single exception logged");
				}
			}
		}
	}
}
