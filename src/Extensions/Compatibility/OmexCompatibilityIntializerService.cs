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
		private readonly ActivitySource m_activitySource;
		private readonly ILoggerFactory m_loggerFactory;

		public OmexCompatibilityIntializerService(ActivitySource activitySource, ILoggerFactory loggerFactory) =>
			(m_activitySource, m_loggerFactory) = (activitySource, loggerFactory);

		Task IHostedService.StartAsync(CancellationToken cancellationToken)
		{
			OmexCompatibilityIntializer.Initialize(m_loggerFactory, m_activitySource);
			return Task.CompletedTask;
		}

		Task IHostedService.StopAsync(CancellationToken cancellationToken) =>
			Task.CompletedTask;
	}
}
