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
		/// TimedScope sub type
		/// </summary>
		public string SubType { get; set; }


		/// <summary>
		/// TimedScope meta data
		/// </summary>
		public string Metadata { get; set; }


		/// <summary>
		/// Activity connected with this TimedScope
		/// </summary>
		public Activity Activity { get; }


		/// <summary>
		/// Indicates if activty was finished
		/// </summary>
		public bool IsFinished { get; private set; }


		/// <summary>
		/// Indicates if activty was started
		/// </summary>
		public bool IsStarted { get; private set; }


		/// <summary>
		/// Creates TimedScope instance
		/// </summary>
		/// <param name="eventSender">event sender to write timedscope end information</param>
		/// <param name="activity">activity connected to this timedscope</param>
		/// <param name="result">TimedScope initial result</param>
		/// <param name="logReplayer">Log replayer that might be used to replay logs in case of error</param>
		protected internal TimedScope(ITimedScopeEventSender eventSender, Activity activity, TimedScopeResult result, ILogEventReplayer? logReplayer = null)
		{
			m_eventSender = eventSender;
			Activity = activity;
			Result = result;
			m_logReplayer = logReplayer;
			SubType = NullPlaceholder;
			Metadata = NullPlaceholder;
			IsStarted = false;
			IsFinished = false;
		}


		/// <summary>
		/// Starts TimedScope activity
		/// </summary>
		public TimedScope Start()
		{
			if (IsStarted)
			{
				throw new InvalidOperationException("Activity already started");
			}

			Activity.Start();
			IsStarted = true;
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

			m_eventSender.LogTimedScopeEndEvent(this);

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


		private readonly ITimedScopeEventSender m_eventSender;
		private const string NullPlaceholder = "null";
		private readonly ILogEventReplayer? m_logReplayer;
	}
}
