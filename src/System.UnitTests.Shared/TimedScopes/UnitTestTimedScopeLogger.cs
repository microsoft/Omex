// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Omex.System.TimedScopes;

namespace Microsoft.Omex.System.UnitTests.Shared.TimedScopes
{
	/// <summary>
	/// Unit test implementation of ITimedScopeLogger
	/// </summary>
	public class UnitTestTimedScopeLogger : ITimedScopeLogger
	{
		private readonly ConcurrentQueue<TimedScopeLogEvent> m_events = new ConcurrentQueue<TimedScopeLogEvent>();

		/// <summary>
		/// Logs the scope start
		/// </summary>
		/// <param name="scope">Scope to log</param>
		public void LogScopeStart(TimedScope scope)
		{
		}

		/// <summary>
		/// Logs the scope end
		/// </summary>
		/// <param name="scope">Scope to log</param>
		/// <param name="data">Correlation data</param>
		public void LogScopeEnd(TimedScope scope, CorrelationData data)
		{
			if (scope == null)
			{
				return;
			}

			TimedScopeLogEvent evt = new TimedScopeLogEvent(scope.Name, scope.SubType,
				scope.MetaData, scope.Result, scope.FailureDescription,
				data.Data(TimedScopeDataKeys.InternalOnly.UserHash),
				scope.Duration ?? TimeSpan.Zero);
			m_events.Enqueue(evt);
		}

		/// <summary>
		/// Gets the last scope event of given name logged
		/// </summary>
		/// <param name="scopeName">Scope name</param>
		/// <returns>The last timed scope log event, null if there is no timed scope event</returns>
		public TimedScopeLogEvent LastTimedScopeEvent(string scopeName)
		{
			return m_events.LastOrDefault(evt => string.Equals(evt.Name, scopeName, StringComparison.Ordinal));
		}

		/// <summary>
		/// Gets the single scope event of given name logged
		/// </summary>
		/// <param name="scopeName">Scope name</param>
		/// <returns>The single timed scope log event, null if there is no or more than one scope event of given name</returns>
		public TimedScopeLogEvent SingleTimedScopeEvent(string scopeName)
		{
			return m_events.SingleOrDefault(evt => string.Equals(evt.Name, scopeName, StringComparison.Ordinal));
		}

		/// <summary>
		/// Logged events
		/// </summary>
		public IEnumerable<TimedScopeLogEvent> Events => m_events;

	}
}
