// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Abstractions.Activities
{
	/// <summary>
	/// Logs duration of activity
	/// </summary>
	/// <remarks>This class will be replaced with Activity after move to net 5, since it will be trackable and disposible</remarks>
	public class TimedScope : IDisposable
	{
		/// <summary>
		/// Creates TimedScope instance
		/// </summary>
		/// <param name="activity">activity connected to this timedscope</param>
		/// <param name="result">TimedScope initial result</param>
		public TimedScope(Activity activity, TimedScopeResult result)
		{
			Activity = activity;
			this.SetResult(result);
		}


		/// <summary>
		/// Starts TimedScope activity
		/// </summary>
		public TimedScope Start()
		{
			s_listener.StartActivity(Activity, this);
			return this;
		}


		/// <summary>
		/// Stop TimedScope and log informations about it
		/// </summary>
		public void Stop() => s_listener.StopActivity(Activity, this);


		internal Activity Activity { get; }


		void IDisposable.Dispose() => Stop();


		private static readonly DiagnosticListener s_listener = new DiagnosticListener("TimedScopesListener");
	}
}
