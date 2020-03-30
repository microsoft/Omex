// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	internal sealed class ActivityObserversIntializer : IHostedService, IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object>>
	{
		private const string ActivityStartEnding = ".Start";

		private const string ActivityStopEnding = ".Stop";

		private static readonly string[] s_eventEndMarkersToListen = new[] {
			ActivityStartEnding,
			ActivityStopEnding,
			// We need to listen for the "Microsoft.AspNetCore.Hosting.HttpRequestIn" event in order to signal Kestrel to create an Activity for the incoming http request.
			// Searching only for RequestIn, in case any other requests follow the same pattern
			"RequestIn",
			// We need to listen for the "System.Net.Http.HttpRequestOut" event in order to create an Activity for the outgoing http requests.
			// Searching only for RequestOut, in case any other requests follow the same pattern
			"RequestOut",
		};

		private readonly IActivityStartObserver[] m_activityStartObservers;
		private readonly IActivityStopObserver[] m_activityStopObservers;
		private readonly LinkedList<IDisposable> m_disposables;
		private IDisposable? m_observerLifetime;

		public ActivityObserversIntializer(
			IEnumerable<IActivityStartObserver> activityStartObservers,
			IEnumerable<IActivityStopObserver> activityStopObservers)
		{
			m_activityStartObservers = activityStartObservers.ToArray();
			m_activityStopObservers = activityStopObservers.ToArray();
			m_disposables = new LinkedList<IDisposable>();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			m_observerLifetime = DiagnosticListener.AllListeners.Subscribe(this);
			return Task.CompletedTask;
		}
		
		public Task StopAsync(CancellationToken cancellationToken)
		{
			foreach (IDisposable disposable in m_disposables)
			{
				disposable.Dispose();
			}

			m_observerLifetime?.Dispose();
			return Task.CompletedTask;
		}

		private bool EventEndsWith(string eventName, string ending) => eventName.EndsWith(ending, StringComparison.Ordinal);

		private bool IsEnabled(string eventName)
		{
			// using foreach instead of Any to avoid creating closure since this method called very often
			foreach (string ending in s_eventEndMarkersToListen)
			{
				if (EventEndsWith(eventName, ending))
				{
					return true;
				}
			}

			return false;
		}

		void IObserver<DiagnosticListener>.OnCompleted() { }

		void IObserver<DiagnosticListener>.OnError(Exception error) { }

		void IObserver<DiagnosticListener>.OnNext(DiagnosticListener value)
		{
			if (m_activityStartObservers.Length != 0 || m_activityStopObservers.Length != 0)
			{
				m_disposables.AddLast(value.Subscribe(this, IsEnabled));
			}
		}

		void IObserver<KeyValuePair<string, object>>.OnCompleted() { }

		void IObserver<KeyValuePair<string, object>>.OnError(Exception error) { }

		void IObserver<KeyValuePair<string, object>>.OnNext(KeyValuePair<string, object> value)
		{
			Activity activity = Activity.Current;
			string eventName = value.Key;

			if (EventEndsWith(eventName, ActivityStartEnding))
			{
				OnActivityStarted(activity, value.Value);
			}
			else if (EventEndsWith(eventName, ActivityStopEnding))
			{
				OnActivityStopped(activity, value.Value);
			}
		}

		private void OnActivityStarted(Activity activity, object payload)
		{
			foreach (IActivityStartObserver startHandler in m_activityStartObservers)
			{
				startHandler.OnStart(activity, payload);
			}
		}

		private void OnActivityStopped(Activity activity, object payload)
		{
			foreach (IActivityStopObserver stopHandler in m_activityStopObservers)
			{
				stopHandler.OnStop(activity, payload);
			}
		}
	}
}
