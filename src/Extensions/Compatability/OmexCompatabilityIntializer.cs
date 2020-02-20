// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Compatability.Logger;
using Microsoft.Omex.Extensions.Compatability.Validation;

namespace Microsoft.Omex.Extensions.Compatability
{
	internal sealed class OmexCompatabilityIntializer : IHostedService
	{
		private readonly ILoggerFactory m_loggerFactory;


		public OmexCompatabilityIntializer(ILoggerFactory loggerFactory) =>
			m_loggerFactory = loggerFactory;


		public Task StartAsync(CancellationToken cancellationToken)
		{
			Code.Initialize(m_loggerFactory);
			ULSLogging.Initialize(m_loggerFactory);
			return Task.CompletedTask;
		}


		public Task StopAsync(CancellationToken cancellationToken) =>
			Task.CompletedTask;
	}
}
