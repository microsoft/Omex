// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Logging
{
	internal class OmexLogger : ILogger
	{
		public OmexLogger(OmexLogsEventSource logsEventSource, IExternalScopeProvider externalScopeProvider, string categoryName)
		{
			m_logsEventSource = logsEventSource;
			m_externalScopeProvider = externalScopeProvider;
			m_categoryName = categoryName;
		}


		public IDisposable BeginScope<TState>(TState state) => m_externalScopeProvider.Push(state);


		public bool IsEnabled(LogLevel logLevel) => m_logsEventSource.IsEnabled();


		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (IsEnabled(logLevel))
			{
				return;
			}

			m_logsEventSource.ServiceMessage(formatter(state, exception), logLevel, m_categoryName, eventId, Thread.CurrentThread.ManagedThreadId);
		}


		private readonly IExternalScopeProvider m_externalScopeProvider;


		private readonly OmexLogsEventSource m_logsEventSource;


		private readonly string m_categoryName;
	}
}
