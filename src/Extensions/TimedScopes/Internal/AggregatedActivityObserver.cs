// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	internal sealed class AggregatedActivityObserver : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object>>, IDisposable
	{
		private readonly IActivityStartObserver[] m_activityStartObservers;
		private readonly IActivityStopObserver[] m_activityStopObservers;
		private readonly LinkedList<IDisposable> m_disposables;


		public AggregatedActivityObserver(
			IEnumerable<IActivityStartObserver> activityStartObservers,
			IEnumerable<IActivityStopObserver> activityStopObservers)
		{
			m_activityStartObservers = activityStartObservers.ToArray();
			m_activityStopObservers = activityStopObservers.ToArray();
			m_disposables = new LinkedList<IDisposable>();
		}

		private void OnActivityStarted(Activity activity)
		{
			foreach (IActivityStartObserver startHandler in m_activityStartObservers)
			{
				startHandler.OnStart(activity);
			}
		}

		private void OnActivityStoped(Activity activity)
		{
			foreach (IActivityStopObserver stopHandler in m_activityStopObservers)
			{
				stopHandler.OnStop(activity);
			}
		}

		private bool IsActivityStart(string eventName) => m_activityStartObservers.Any() && eventName.EndsWith(".Start", StringComparison.Ordinal);

		private bool IsActivityStop(string eventName) => m_activityStopObservers.Any() && eventName.EndsWith(".Stop", StringComparison.Ordinal);

		private bool IsEnabled(string eventName) => IsActivityStart(eventName) || IsActivityStop(eventName);

		void IObserver<DiagnosticListener>.OnCompleted() { }

		void IObserver<DiagnosticListener>.OnError(Exception error) { }

		void IObserver<DiagnosticListener>.OnNext(DiagnosticListener value) => m_disposables.AddLast(value.Subscribe(this, IsEnabled));

		void IObserver<KeyValuePair<string, object>>.OnCompleted() { }

		void IObserver<KeyValuePair<string, object>>.OnError(Exception error) { }

		void IObserver<KeyValuePair<string, object>>.OnNext(KeyValuePair<string, object> value)
		{
			Activity activity = Activity.Current;

			if (IsActivityStart(value.Key))
			{
				OnActivityStarted(activity);
			}

			if (IsActivityStop(value.Key))
			{
				OnActivityStoped(activity);
			}
		}

		public void Dispose()
		{
			foreach (IDisposable disposable in m_disposables)
			{
				disposable.Dispose();
			}
		}
	}
}
