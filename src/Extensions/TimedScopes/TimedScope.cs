// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	/// <summary>Logs duration of activity</summary>
	public class TimedScope : IDisposable
	{
		/// <summary>TimedScope result</summary>
		public TimedScopeResult Result { get; set; }


		/// <summary>TimedScope sub tipe</summary>
		public string SubType { get; set; }


		/// <summary>TimedScope meta data</summary>
		public string MetaData { get; set; }

		/// <summary>Indicates if activty was </summary>
		public bool IsFinished { get; private set; }


		internal TimedScope(TimedScopeEventSource eventSource, Activity activity, string serviceName, TimedScopeResult result, ILogReplayer? logReplayer)
		{
			m_eventSource = eventSource;
			m_logReplayer = logReplayer;
			m_serviceName = serviceName;
			m_activity = activity;
			Result = result;
			SubType = NullPlaceholder;
			MetaData = NullPlaceholder;
			IsFinished = false;
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
			if (IsFinished)
			{
				return;
			}

			IsFinished = true;
			m_activity.Stop();
			m_eventSource.LogEvent(
				name: m_activity.OperationName,
				subtype: SubType,
				metadata: MetaData,
				userHash: m_activity.GetUserHash(), //Breaking Change: feild not set
				serviceName: m_serviceName,
				result: Result,
				correlationId: m_activity.Id,
				durationMs: m_activity.Duration.TotalMilliseconds,
				isTransaction: m_activity.IsTransaction()); //Breaking Change: feild not set

			if (m_logReplayer != null && ShouldReplayEvents)
			{
				m_logReplayer.ReplayLogs(m_activity);
			}
		}


		void IDisposable.Dispose() => Stop();


		private bool ShouldReplayEvents =>
			Result switch
			{
				TimedScopeResult.SystemError => true,
				_ => false,
			};


		private readonly Activity m_activity;
		private readonly TimedScopeEventSource m_eventSource;
		private readonly ILogReplayer? m_logReplayer;
		private readonly string m_serviceName;
		private const string NullPlaceholder = "null";
	}
}
