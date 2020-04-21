// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Wraps service run into HostedService
	/// </summary>
	internal sealed class OmexHostedService : IHostedService
	{
		private readonly IHostApplicationLifetime m_lifetime;
		private readonly IOmexServiceRegistrator m_runner;
		private readonly ILogger<OmexHostedService> m_logger;
		private readonly CancellationTokenSource m_tokenSource;

		public OmexHostedService(IOmexServiceRegistrator runner, IHostApplicationLifetime lifetime, ILogger<OmexHostedService> logger)
		{
			m_runner = runner;
			m_lifetime = lifetime;
			m_logger = logger;
			m_tokenSource = new CancellationTokenSource();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			m_lifetime.ApplicationStarted.Register(OnServiceStarted);
			m_lifetime.ApplicationStopping.Register(OnServiceStopping);
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

		// would be triggered after for all hosted services StartAsync method completed
		// this is important since there might be some initialization logic there (ex. register Activity listeners)
		// and service should be started after all of them
		private void OnServiceStarted() => RegisterServiceAsync().ContinueWith(OnRunnerCompleted);

		private void OnServiceStopping()
		{
			// call to Cancel on canceled token source will throw
			if (m_tokenSource.IsCancellationRequested)
			{
				return;
			}

			// cancel service initialization if host is stopping
			m_tokenSource.Cancel();
		}

		private async Task RegisterServiceAsync()
		{
			try
			{
				await m_runner.RegisterAsync(m_tokenSource.Token).ConfigureAwait(false);

				m_logger.LogInformation(Tag.Create(), "ServiceFabricHost initialized");
			}
			catch (Exception e)
			{
				m_logger.LogCritical(Tag.Create(), e, "Exception during ServiceFabricHost initialization");

				// stop host if initialization process failed
				m_lifetime.StopApplication();

				throw;
			}
		}

		private Task OnRunnerCompleted(Task task)
		{
			if (task.IsFaulted)
			{
				m_logger.LogCritical(Tag.Create(), task.Exception, "Service registration stoped due to exeption");
			}
			else if (task.IsCanceled)
			{
				m_logger.LogInformation(Tag.Create(), "Service registration canceled");
			}
			else
			{
				m_logger.LogInformation(Tag.Create(), "Service registration stopped");
			}

			return task;
		}
	}
}
