// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Hosting
{
	internal class ExceptionLoggingHostedService : IHostedService
	{
		private readonly ILogger<ExceptionLoggingHostedService> m_logger;

		public ExceptionLoggingHostedService(ILogger<ExceptionLoggingHostedService> logger)
			=> m_logger = logger;

		public Task StartAsync(CancellationToken cancellationToken)
		{
			AppDomain.CurrentDomain.FirstChanceException += FirstChanceException;
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			AppDomain.CurrentDomain.FirstChanceException -= FirstChanceException;
			return Task.CompletedTask;
		}

		private void FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
		{
			m_logger.LogError(Tag.Create(), e.Exception, "Exception during application execution");
		}
	}
}
