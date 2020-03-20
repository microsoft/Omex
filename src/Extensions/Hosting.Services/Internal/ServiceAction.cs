// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal sealed class ServiceAction<TService> : IServiceAction<TService>
	{
		public ServiceAction(Func<TService, CancellationToken, Task> action) =>
			m_action = action;

		public Task RunAsync(TService service, CancellationToken cancellationToken) =>
			m_action(service, cancellationToken);

		private readonly Func<TService, CancellationToken, Task> m_action;
	}
}
