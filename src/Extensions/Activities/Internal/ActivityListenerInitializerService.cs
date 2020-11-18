// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.Activities
{
	internal class ActivityListenerInitializerService : IHostedService
	{
		private readonly IActivityStartObserver[] m_activityStartObservers;
		private readonly IActivityStopObserver[] m_activityStopObservers;
		private readonly IActivityListenerConfigurator m_configurator;
		private ActivityListener? m_listener;

		public ActivityListenerInitializerService(
			IEnumerable<IActivityStartObserver> activityStartObservers,
			IEnumerable<IActivityStopObserver> activityStopObservers,
			IActivityListenerConfigurator configurator)
		{
			m_activityStartObservers = activityStartObservers.ToArray();
			m_activityStopObservers = activityStopObservers.ToArray();
			m_configurator = configurator;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			m_listener = new ActivityListener();
			m_listener.ActivityStarted += ActivityStart;
			m_listener.ActivityStopped += ActivityStopped;
			m_listener.Sample += Sample;
			m_listener.SampleUsingParentId += SampleUsingParentId;
			m_listener.ShouldListenTo += ShouldListenTo;
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			m_listener?.Dispose();
			return Task.CompletedTask;
		}


		private void ActivityStart(Activity activity)
		{
			foreach (IActivityStartObserver startHandler in m_activityStartObservers)
			{
				startHandler.OnStart(activity);
			}
		}

		private void ActivityStopped(Activity activity)
		{
			foreach (IActivityStopObserver stopHandler in m_activityStopObservers)
			{
				stopHandler.OnStop(activity);
			}
		}

		private bool ShouldListenTo(ActivitySource activity) =>
			m_configurator.ShouldListenTo(activity);

		private ActivitySamplingResult SampleUsingParentId(ref ActivityCreationOptions<string> options) =>
			m_configurator.SampleUsingParentId(ref options);

		private ActivitySamplingResult Sample(ref ActivityCreationOptions<ActivityContext> options) =>
			m_configurator.Sample(ref options);
	}
}
