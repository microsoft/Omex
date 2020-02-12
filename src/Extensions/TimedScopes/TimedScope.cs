// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
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


		/// <summary>Indicates if activty was finished</summary>
		public bool IsFinished { get; private set; }


		/// <summary>Activity connected with this TimedScope</summary>
		public Activity Activity { get; private set; }


		internal TimedScope(ITimedScopeEventSource eventSource, Activity activity, TimedScopeResult result, ILogReplayer? logReplayer)
		{
			m_eventSource = eventSource;
			m_logReplayer = logReplayer;
			Activity = activity;
			Result = result;
			SubType = NullPlaceholder;
			MetaData = NullPlaceholder;
			IsFinished = false;
		}


		/// <summary>Start TimedScope</summary>
		public TimedScope Start()
		{
			Activity.Start();
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

			Activity.Stop();

			m_eventSource.LogTimedScopeEndEvent(this);

			if (m_logReplayer != null && ShouldReplayEvents)
			{
				m_logReplayer.ReplayLogs(Activity);
			}
		}


		void IDisposable.Dispose() => Stop();


		private bool ShouldReplayEvents =>
			Result switch
			{
				TimedScopeResult.SystemError => true,
				_ => false,
			};


		private readonly ITimedScopeEventSource m_eventSource;
		private const string NullPlaceholder = "null";
		private readonly ILogReplayer? m_logReplayer;
	}
}
