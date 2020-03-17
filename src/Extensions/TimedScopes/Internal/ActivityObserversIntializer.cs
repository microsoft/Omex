// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	internal sealed class ActivityObserversIntializer : IHostedService
	{
		public ActivityObserversIntializer(AggregatedActivityObserver observer) => m_observer = observer;
		
		public Task StartAsync(CancellationToken cancellationToken)
		{
			m_observerLifetime = DiagnosticListener.AllListeners.Subscribe(m_observer);
			return Task.CompletedTask;
		}
		
		public Task StopAsync(CancellationToken cancellationToken)
		{
			m_observer.Dispose();
			m_observerLifetime?.Dispose();
			return Task.CompletedTask;
		}

		private IDisposable? m_observerLifetime;
		private readonly AggregatedActivityObserver m_observer;
	}
}
