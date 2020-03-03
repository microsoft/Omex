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
		private readonly IOmexServiceRunner m_runner;
		private readonly ILogger<OmexHostedService> m_logger;
		private readonly CancellationTokenSource m_tokenSource;

		public OmexHostedService(IOmexServiceRunner runner, IHostApplicationLifetime lifetime, ILogger<OmexHostedService> logger)
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


		private void OnServiceStarted()
		{
			RunServiceAsync()
				.ContinueWith(OnRunnerCompleted)
				.ConfigureAwait(false);
		}


		private void OnServiceStopping()
		{
			if (m_tokenSource.IsCancellationRequested)
			{
				return;
			}

			m_tokenSource.Cancel();
		}


		private async Task RunServiceAsync()
		{
			try
			{
				ConfiguredTaskAwaitable task = m_runner.RunServiceAsync(m_tokenSource.Token).ConfigureAwait(false);

				m_logger.LogInformation(Tag.Create(), "ServiceFabricHost initialized");

				await task;
			}
			catch (Exception e)
			{
				m_logger.LogCritical(Tag.Create(), e, "Exception during ServiceFabricHost initialization");
			}
		}


		private Task OnRunnerCompleted(Task task)
		{
			if (task.IsFaulted)
			{
				m_logger.LogCritical(Tag.Create(), task.Exception, "Service stoped due to exeption");
			}
			else if (task.IsCanceled)
			{
				m_logger.LogInformation(Tag.Create(), "Service canceled");
			}
			else
			{
				m_logger.LogInformation(Tag.Create(), "Service stoped");
			}

			m_lifetime.StopApplication();
			return Task.CompletedTask;
		}
	}
}
