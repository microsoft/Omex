// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Compatibility.Logger;
using Microsoft.Omex.Extensions.Compatibility.TimedScopes;
using Microsoft.Omex.Extensions.Compatibility.Validation;

namespace Microsoft.Omex.Extensions.Compatibility
{
	internal sealed class OmexCompatibilityIntializer : IHostedService
	{
		public OmexCompatibilityIntializer(ITimedScopeProvider timedScopeProvider, ILoggerFactory loggerFactory) =>
			(m_timedScopeProvider, m_loggerFactory) = (timedScopeProvider, loggerFactory);

		public Task StartAsync(CancellationToken cancellationToken)
		{
			TimedScopeDefinitionExtensions.Initialize(m_timedScopeProvider);
			Code.Initialize(m_loggerFactory);
			ULSLogging.Initialize(m_loggerFactory);

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken) =>
			Task.CompletedTask;

		private readonly ITimedScopeProvider m_timedScopeProvider;
		private readonly ILoggerFactory m_loggerFactory;
	}
}
