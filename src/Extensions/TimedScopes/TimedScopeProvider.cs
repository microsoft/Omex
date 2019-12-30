// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Omex.Extensions.Abstractions;

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


	/// <summary>Interface to create TimedScope</summary>
	public interface ITimedScopeProvider
	{
		/// <summary>Create and start TimedScope</summary>
		TimedScope Start(string name, TimedScopeResult result = TimedScopeResult.SystemError);
	}


	/// <summary>Class to log duration of activity</summary>
	public class TimedScope : IDisposable
	{
		/// <summary>TimedScope result</summary>
		public TimedScopeResult Result { get; set; }


		/// <summary>TimedScope sub tipe</summary>
		public string SubType { get; set; }


		/// <summary>TimedScope meta data</summary>
		public string MetaData { get; set; }

		private readonly Activity m_activity;
		private readonly TimedScopeEventSource m_eventSource;
		private readonly string m_serviceName;
		private const string NullPlaceholder = "null";


		internal TimedScope(TimedScopeEventSource eventSource, string serviceName, string name, TimedScopeResult result)
		{
			m_eventSource = eventSource;
			m_serviceName = serviceName;
			m_activity = new Activity(name);
			Result = result;
			SubType = NullPlaceholder;
			MetaData = NullPlaceholder;
		}


		/// <summary>Start TimedScope</summary>
		public TimedScope Start()
		{
			m_activity.Start();
			return this;
		}


		/// <summary>Stop TimedScope and log informations about it</summary>
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


		void IDisposable.Dispose() => Stop();
	}
}
