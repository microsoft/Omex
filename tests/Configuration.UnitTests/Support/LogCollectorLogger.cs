// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Configuration.UnitTests.Support
{
	/// <summary>
	/// This implementation of the ILogger instace collect all the log registered within the instance
	/// to return them for test purposes.
	/// </summary>
	/// <typeparam name="TInstance">The class for which the logger instance will log.</typeparam>
	internal class LogCollectorLogger<TInstance> : ILogger<TInstance>
	{
		private readonly IDictionary<LogLevel, IList<string>> m_logMessages = new Dictionary<LogLevel, IList<string>>();

		/// <inheritdoc/>
		public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();

		/// <inheritdoc/>
		public bool IsEnabled(LogLevel logLevel) => logLevel == LogLevel.Error;

		/// <inheritdoc/>
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			string logMessage = formatter(state, exception);

			if (m_logMessages.ContainsKey(logLevel))
			{
				m_logMessages[logLevel].Add(logMessage);
			}

			m_logMessages.Add(logLevel, new List<string>() { formatter(state, exception) });
		}

		/// <summary>
		/// Returns the log messages registered with the instance.
		/// </summary>
		/// <param name="level">The desired log level. If null, all the log messages will be returned.</param>
		/// <returns>The logged messages within this instance.</returns>
		public IEnumerable<string> GetLogMessages(LogLevel? level = null)
		{
			if (level.HasValue && m_logMessages.TryGetValue(level.Value, out IList<string>? value))
			{
				return value ?? Array.Empty<string>();
			}

			return m_logMessages.Values.SelectMany(messages => messages);
		}
	}
}
