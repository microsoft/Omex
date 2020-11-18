// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Compatibility
{
	internal sealed class OmexCompatibilityIntializerService : IHostedService
	{
		private readonly ActivitySource m_timedScopeProvider;
		private readonly ILoggerFactory m_loggerFactory;

		public OmexCompatibilityIntializerService(ActivitySource timedScopeProvider, ILoggerFactory loggerFactory) =>
			(m_timedScopeProvider, m_loggerFactory) = (timedScopeProvider, loggerFactory);

		Task IHostedService.StartAsync(CancellationToken cancellationToken)
		{
			OmexCompatibilityIntializer.Initialize(m_loggerFactory, m_timedScopeProvider);
			return Task.CompletedTask;
		}

		Task IHostedService.StopAsync(CancellationToken cancellationToken) =>
			Task.CompletedTask;
	}
}
