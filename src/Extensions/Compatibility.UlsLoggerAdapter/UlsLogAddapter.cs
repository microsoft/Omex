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
		private static readonly Func<string, Exception, string> s_formatter = (message, exception) => message; // emulating behavior of standard formatter to avoid duplicating exception stack trace
		private readonly ILoggerFactory m_loggerFactory;
		private readonly ILogger<UlsLogAddapter> m_logger;
		private readonly ConcurrentDictionary<string, ILogger> m_loggersDictionary;

		public UlsLogAddapter(ILoggerFactory loggerFactory, ILogger<UlsLogAddapter> logger)
		{
			m_loggerFactory = loggerFactory;
			m_logger = logger;
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

		private void LogEvent(object sender, LogEventArgs e)
		{
			m_loggersDictionary.GetOrAdd(e.CategoryId.Name, m_loggerFactory.CreateLogger).Log(
				Convert(e.Level),
				(int)e.TagId,
				e.FullMessage,
				e is ReportExceptionEventArgs errorArgs ? errorArgs.Exception : null,
				s_formatter);
		}

		private LogLevel Convert(Level level)
		{
			if (Levels.Error == level)
			{
				return LogLevel.Error;
			}
			else if (Levels.Warning == level)
			{
				return LogLevel.Warning;
			}
			else if (Levels.Info == level)
			{
				return LogLevel.Information;
			}
			else if (Levels.Verbose == level)
			{
				return LogLevel.Debug;
			}
			else if (Levels.Spam == level)
			{
				return LogLevel.Trace;
			}
			else
			{
				m_logger.LogError((int)TaggingUtilities.ReserveTag(0), "Unexpected log level '{0}'", level);
				return LogLevel.Error;
			}
		}
	}
}
