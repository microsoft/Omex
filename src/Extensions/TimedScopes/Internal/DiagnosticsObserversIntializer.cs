// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	/// <summary>
	/// Hosted service for listening to diagnostic events
	/// </summary>
	/// <remarks>
	/// Should be changed after .net 5 release https://github.com/dotnet/designs/pull/98
	/// </remarks>
	internal sealed class DiagnosticsObserversIntializer : IHostedService, IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object>>
	{
		/// <summary>
		/// Ending of the Activity Start event
		/// </summary>
		internal static readonly string ActivityStartEnding = ".Start";

		/// <summary>
		/// Ending of the Activity Stop event
		/// </summary>
		internal static readonly string ActivityStopEnding = ".Stop";

		/// <summary>
		/// Ending of the exception events
		/// </summary>
		internal static readonly string ExceptionEventEnding = "Exception";

		private static readonly string[] s_eventEndMarkersToListen = new[] {
			ActivityStartEnding,
			ActivityStopEnding,
			ExceptionEventEnding,
			// We need to listen for the "Microsoft.AspNetCore.Hosting.HttpRequestIn" event in order to signal Kestrel to create an Activity for the incoming http request.
			// Searching only for RequestIn, in case any other requests follow the same pattern
			"RequestIn",
			// We need to listen for the "System.Net.Http.HttpRequestOut" event in order to create an Activity for the outgoing http requests.
			// Searching only for RequestOut, in case any other requests follow the same pattern
			"RequestOut"
		};

		private static bool EventEndsWith(string eventName, string ending) => eventName.EndsWith(ending, StringComparison.Ordinal);

		private static bool IsEnabled(string eventName)
		{
			// Using foreach instead of Any to avoid creating closure since this method is called very often
			foreach (string ending in s_eventEndMarkersToListen)
			{
				if (EventEndsWith(eventName, ending))
				{
					return true;
				}
			}

			return false;
		}

		private readonly IActivityStartObserver[] m_activityStartObservers;
		private readonly IActivityStopObserver[] m_activityStopObservers;
		private readonly ILogger<DiagnosticsObserversIntializer> m_logger;
		private readonly LinkedList<IDisposable> m_disposables;
		private IDisposable? m_observerLifetime;

		public DiagnosticsObserversIntializer(
			IEnumerable<IActivityStartObserver> activityStartObservers,
			IEnumerable<IActivityStopObserver> activityStopObservers,
			ILogger<DiagnosticsObserversIntializer> logger)
		{
			m_activityStartObservers = activityStartObservers.ToArray();
			m_activityStopObservers = activityStopObservers.ToArray();
			m_logger = logger;
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

		public void OnCompleted() { }

		public void OnError(Exception error) =>
			m_logger.LogError(Tag.Create(), error, "Exception in diagnostic events provider");

		public void OnNext(DiagnosticListener value)
		{
			if (m_activityStartObservers.Length != 0 || m_activityStopObservers.Length != 0)
			{
				m_disposables.AddLast(value.Subscribe(this, IsEnabled));
			}
		}

		public void OnNext(KeyValuePair<string, object> value)
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
			else if (EventEndsWith(eventName, ExceptionEventEnding))
			{
				OnException(eventName, value.Value);
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

		private void OnException(string eventName, object payload) =>
			m_logger.LogError(
				Tag.Create(),
				ExtractExceptionFromPayload(payload),
				"Exception diagnostic event '{0}' with payload '{1}'", eventName, payload);


		// Made internal for unit testing
		internal static Exception? ExtractExceptionFromPayload(object? payload)
		{
			if (payload == null)
			{
				return null;
			}
			else if (payload is Exception exception)
			{
				return exception;
			}
			else
			{
				// Attempting to find exception property since payload often use private classes,
				// It would be completly removed after .net 5 release.
				// It's definetly not ideal identification
				// DataAdapters doing it by parameter name https://github.com/aspnet/EventNotification/blob/28b77e7fb51b30797ce34adf86748c98c040985e/src/Microsoft.Extensions.DiagnosticAdapter/Internal/ProxyMethodEmitter.cs#L69
				// Sample class with payload https://github.com/dotnet/runtime/blob/master/src/libraries/System.Net.Http/src/System/Net/Http/DiagnosticsHandler.cs#L181
				Type payloadType = payload.GetType();
				if (!s_exceptionProperties.TryGetValue(payloadType, out PropertyInfo? propertyInfo))
				{
					propertyInfo = payload.GetType()
						.GetProperties()
						.FirstOrDefault(p => typeof(Exception).IsAssignableFrom(p.PropertyType));

					s_exceptionProperties.TryAdd(payloadType, propertyInfo);
				}

				return propertyInfo?.GetValue(payload) as Exception;
			}
		}

		private static readonly ConcurrentDictionary<Type, PropertyInfo?> s_exceptionProperties =
			new ConcurrentDictionary<Type, PropertyInfo?>();
	}
}
