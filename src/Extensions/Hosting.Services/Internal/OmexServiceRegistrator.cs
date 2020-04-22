// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.Accessors;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal abstract class OmexServiceRegistrator<TService, TContext> : IOmexServiceRegistrator
		where TService : IServiceFabricService<TContext>
		where TContext : ServiceContext
	{
		protected readonly string ApplicationName;

		protected readonly IAccessorSetter<TContext> ContextAccessor;

		public IEnumerable<IListenerBuilder<TService>> ListenerBuilders { get; }

		public IEnumerable<IServiceAction<TService>> ServiceActions { get; }

		public OmexServiceRegistrator(
			IHostEnvironment environment,
			IAccessorSetter<TContext> contextAccessor,
			IEnumerable<IListenerBuilder<TService>> listenerBuilders,
			IEnumerable<IServiceAction<TService>> serviceActions)
		{
			ApplicationName = environment.ApplicationName;
			ContextAccessor = contextAccessor;
			ListenerBuilders = listenerBuilders;
			ServiceActions = serviceActions;
		}

		public abstract Task RegisterAsync(CancellationToken cancellationToken);
	}
}
