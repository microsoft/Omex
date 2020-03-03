// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	/// <summary>
	/// Logs duration of activity
	/// </summary>
	public class TimedScope : IDisposable
	{
		/// <summary>
		/// TimedScope result
		/// </summary>
		public TimedScopeResult Result { get; set; }


		/// <summary>
		/// TimedScope sub tipe
		/// </summary>
		public string SubType { get; set; }


		/// <summary>
		/// TimedScope meta data
		/// </summary>
		public string MetaData { get; set; }


		/// <summary>
		/// Activity connected with this TimedScope
		/// </summary>
		public Activity Activity { get; }


		/// <summary>
		/// Indicates if activty was finished
		/// </summary>
		public bool IsFinished { get; private set; }


		/// <summary>
		/// Creates TimedScope instance
		/// </summary>
		/// <param name="eventSource">event source to write timedscope end information</param>
		/// <param name="activity">activity connected to this timedscope</param>
		/// <param name="result">TimedScope initial result</param>
		/// <param name="logReplayer">Log replayer that might be used to replay logs in case of error</param>
		protected internal TimedScope(ITimedScopeEventSender eventSource, Activity activity, TimedScopeResult result, ILogEventReplayer? logReplayer = null)
		{
			m_eventSource = eventSource;
			m_logReplayer = logReplayer;
			Activity = activity;
			Result = result;
			SubType = NullPlaceholder;
			MetaData = NullPlaceholder;
			IsFinished = false;
		}


		/// <summary>
		/// Starts TimedScope activity
		/// </summary>
		protected internal TimedScope Start()
		{
			Activity.Start();
			return this;
		}


		/// <summary>
		/// Stop TimedScope and log informations about it
		/// </summary>
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


		private readonly ITimedScopeEventSender m_eventSource;
		private const string NullPlaceholder = "null";
		private readonly ILogEventReplayer? m_logReplayer;
	}
}
