// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Microsoft.Omex.Extensions.Logging.Internal
{
	/// <summary>
	/// InitLogger class to be used when Ulslogging is not ready
	/// </summary>
	public class InitLogger : ILogger
	{
		private readonly ILogger m_console;
		private readonly ILogger m_omexLogger;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="options">Omex logging options</param>
		public InitLogger(OmexLoggingOptions options)
		{
			ConsoleLoggerProvider provider = new ConsoleLoggerProvider(new LoggingOptionsMonitor<ConsoleLoggerOptions>(new ConsoleLoggerOptions { }));
			OmexLogEventSender prov = new OmexLogEventSender(ServiceInitializationEventSource.Instance, new BasicMachineInformation(),
				new EmptyServiceContext(), Options.Create(options));

			m_console = provider.CreateLogger("Initilisation-console");
			m_omexLogger = new OmexLoggerProvider(prov, new LoggerExternalScopeProvider()).CreateLogger("Init-omex-logger");
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public InitLogger()
		{
			ConsoleLoggerProvider provider = new ConsoleLoggerProvider(new LoggingOptionsMonitor<ConsoleLoggerOptions>(new ConsoleLoggerOptions { }));

			OmexLogEventSender prov = new OmexLogEventSender(ServiceInitializationEventSource.Instance, new BasicMachineInformation(),
				new EmptyServiceContext(), Options.Create(new OmexLoggingOptions { }));

			m_console = provider.CreateLogger("Initilisation-console");
			m_omexLogger = new OmexLoggerProvider(prov, new LoggerExternalScopeProvider()).CreateLogger("Init-omex-logger");
		}

		/// <inheritdoc/>
		public IDisposable BeginScope<TState>(TState state)
		{
			return m_console.BeginScope(state) ?? m_omexLogger.BeginScope(state);
		}

		/// <inheritdoc/>
		public bool IsEnabled(LogLevel logLevel) => m_console.IsEnabled(logLevel) && m_omexLogger.IsEnabled(logLevel);

		/// <inheritdoc/>
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			m_console.Log(logLevel, eventId, state, exception, formatter);
			m_omexLogger.Log(logLevel, eventId, state, exception, formatter);
		}

		private class LoggingOptionsMonitor<T> : IOptionsMonitor<T> where T : class, new()
		{
			public LoggingOptionsMonitor(T currentValue)
			{
				CurrentValue = currentValue;
			}

			public T Get(string name)
			{
				return CurrentValue;
			}

			public IDisposable OnChange(Action<T, string> listener)
			{
				throw new NotImplementedException();
			}

			public T CurrentValue { get; }
		}
	}
}
