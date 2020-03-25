// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal sealed class ServiceAction<TService> : IServiceAction<TService>
	{
		public ServiceAction(IServiceProvider provider, Func<IServiceProvider, TService, CancellationToken, Task> action) =>
			(m_provider, m_action) = (provider, action);

		public Task RunAsync(TService service, CancellationToken cancellationToken) =>
			m_action(m_provider, service, cancellationToken);

		private readonly Func<IServiceProvider, TService, CancellationToken, Task> m_action;
		private readonly IServiceProvider m_provider;
	}
}
