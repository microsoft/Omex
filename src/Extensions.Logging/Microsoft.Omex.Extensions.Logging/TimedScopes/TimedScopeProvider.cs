// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Omex.Extensions.Logging.Diagnostics;

namespace Microsoft.Omex.Extensions.Logging.TimedScopes
{
	internal class TimedScopeProvider : ITimedScopeProvider
	{
		public TimedScopeProvider(IMachineInformation machineInformation, TimedScopeEventSource eventSource)
		{
			m_serviceName = machineInformation.ServiceName;
			m_eventSource = eventSource;
		}

		public TimedScope Start(string name, TimedScopeResult result) =>
			new TimedScope(m_eventSource, m_serviceName, name, result);

		private readonly TimedScopeEventSource m_eventSource;
		private readonly string m_serviceName;
	}

	public interface ITimedScopeProvider
	{
		TimedScope Start(string name, TimedScopeResult result = TimedScopeResult.SystemError);
	}

	public class TimedScope : IDisposable
	{
		public TimedScopeResult Result { get; set; }
		public string SubType { get; set; }
		public string MetaData { get; set; }

		private readonly Activity m_activity;
		private readonly TimedScopeEventSource m_eventSource;
		private readonly string m_serviceName;
		private const string NullPlaceholder = "null";


		public TimedScope(TimedScopeEventSource eventSource, string serviceName, string name, TimedScopeResult result)
		{
			m_eventSource = eventSource;
			m_serviceName = serviceName;
			m_activity = new Activity(name);
			Result = result;
			SubType = NullPlaceholder;
			MetaData = NullPlaceholder;
		}


		public TimedScope Start()
		{
			m_activity.Start();
			return this;
		}


		public void Stop()
		{
			m_activity.Stop();
			m_eventSource.LogEvent(
				name: m_activity.OperationName,
				subtype: SubType,
				metadata: MetaData,
				userHash: ,
				result: Result,
				correlationId: m_activity.Id,
				durationMs: m_activity.Duration.TotalMilliseconds,
				isTransaction: );
		}


		public void Dispose() => Stop();
	}
}
