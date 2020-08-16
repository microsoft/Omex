// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Hosting
{
	internal class HostWrapperWithExceptionLogging : IHost
	{
		private readonly IHost m_host;

		private readonly ILogger<HostWrapperWithExceptionLogging> m_logger;

		public HostWrapperWithExceptionLogging(IHost host)
		{
			m_host = host;
			m_logger = host.Services.GetRequiredService<ILogger<HostWrapperWithExceptionLogging>>();
		}

		public IServiceProvider Services => m_host.Services;

		public void Dispose()
		{
			try
			{
				m_host.Dispose();
			}
			catch (Exception exception)
			{
				m_logger.LogCritical(Tag.Create(), exception, "Dispose failed with exception");
				throw;
			}
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				await m_host.StartAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException operationCanceledException)
			{
				m_logger.LogError(Tag.Create(), operationCanceledException, "StartAsync canceled");
				throw;
			}
			catch (Exception exception)
			{
				m_logger.LogCritical(Tag.Create(), exception, "StartAsync failed with exception");
				throw;
			}
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			try
			{
				await m_host.StopAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (AggregateException aggragatedException)
			{
				foreach (Exception exception in aggragatedException.InnerExceptions)
				{
					switch (exception)
					{
						case OperationCanceledException operationCanceledException:
							m_logger.LogError(Tag.Create(), operationCanceledException, "StopAsync canceled");
							break;
						default:
							m_logger.LogCritical(Tag.Create(), exception, "StopAsync failed with exception");
							break;
					}
				}

				throw;
			}
			catch (Exception exception)
			{
				m_logger.LogCritical(Tag.Create(), exception, "Unexpected failure in StopAsync that is not wrapped in AggragatedException");
				throw;
			}
		}
	}
}
