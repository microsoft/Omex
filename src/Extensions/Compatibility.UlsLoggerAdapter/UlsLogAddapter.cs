// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.System.Logging;

namespace Microsoft.Omex.Extensions.Compatibility.UlsLoggerAdapter
{
	/// <summary>
	/// Hosted service subscribes to log events from Microsoft.Omex.System.UlsLogger and propagates them to ILogger
	/// </summary>
	internal class UlsLogAddapter : IHostedService
	{
		private static readonly Func<string, Exception?, string> s_formatter = (message, exception) => message; // emulating behavior of standard formatter to avoid duplicating exception stack trace
		private readonly ILoggerFactory m_loggerFactory;
		private readonly ConcurrentDictionary<string, ILogger> m_loggersDictionary;

		public UlsLogAddapter(ILoggerFactory loggerFactory)
		{
			m_loggerFactory = loggerFactory;
			m_loggersDictionary = new ConcurrentDictionary<string, ILogger>(StringComparer.OrdinalIgnoreCase);
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			ULSLogging.LogEvent += LogEvent;
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			ULSLogging.LogEvent -= LogEvent;
			return Task.CompletedTask;
		}

		private void LogEvent(object? sender, LogEventArgs e)
		{
			m_loggersDictionary.GetOrAdd(e.CategoryId.Name, m_loggerFactory.CreateLogger).Log(
				Convert(e.Level),
				(int)e.TagId,
				e.FullMessage,
				e is ReportExceptionEventArgs errorArgs ? errorArgs.Exception : null,
				s_formatter);
		}

		private LogLevel Convert(Level level) =>
			level.LogLevel switch
			{
				Levels.LogLevel.Error => LogLevel.Error,
				Levels.LogLevel.Warning => LogLevel.Warning,
				Levels.LogLevel.Info => LogLevel.Information,
				Levels.LogLevel.Verbose => LogLevel.Debug,
				Levels.LogLevel.Spam => LogLevel.Trace,
				_ => LogLevel.Error // treat unsupported levels as Error to ensure that they are not skipped
			};
	}
}
