// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal sealed class ServiceAction<TContext> : IServiceAction<TContext>
		where TContext : ServiceContext
	{
		public ServiceAction(IServiceProvider provider, Func<IServiceProvider, CancellationToken, Task> action) =>
			(m_provider, m_action) = (provider, action);

		public Task RunAsync(CancellationToken cancellationToken) =>
			m_action(m_provider, cancellationToken);

		private readonly Func<IServiceProvider, CancellationToken, Task> m_action;
		private readonly IServiceProvider m_provider;
	}
}
