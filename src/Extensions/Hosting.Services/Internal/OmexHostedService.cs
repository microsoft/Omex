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
	internal sealed class OmexHostedService : BackgroundService
	{
		private readonly IOmexServiceRunner m_runner;
		private readonly IHostLifetime m_lifetime;
		private readonly ILogger<OmexHostedService> m_logger;

		public OmexHostedService(IOmexServiceRunner runner, IHostLifetime lifetime, ILogger<OmexHostedService> logger)
		{
			m_runner = runner;
			m_lifetime = lifetime;
			m_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken token)
		{
			try
			{
				ConfiguredTaskAwaitable task = m_runner.RunServiceAsync(token)
					.ContinueWith(OnServiceStop)
					.ConfigureAwait(false);

				m_logger.LogInformation(Tag.Create(), "ServiceFabricHost initialized");

				await task;
			}
			catch (Exception e)
			{
				m_logger.LogCritical(Tag.Create(), e, "Exception during ServiceFabricHost execution");
				throw;
			}
		}

		private void OnServiceStop(Task task) => m_lifetime.StopAsync(CancellationToken.None);
	}
}
