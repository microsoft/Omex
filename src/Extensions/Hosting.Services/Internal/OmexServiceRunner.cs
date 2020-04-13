// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal abstract class OmexServiceRunner<TService, TContext> : IOmexServiceRunner
		where TService : class, IServiceFabricService<TContext>
		where TContext : ServiceContext
	{
		protected readonly string ApplicationName;

		protected readonly IAccessorSetter<TContext> ContextAccessor;

		protected readonly IAccessorSetter<TService> ServiceAccessor;

		public IEnumerable<IListenerBuilder<TContext>> ListenerBuilders { get; }

		public IEnumerable<IServiceAction<TContext>> ServiceActions { get; }

		public OmexServiceRunner(
			IHostEnvironment environment,
			IAccessorSetter<TService> serviceAccessor,
			IAccessorSetter<TContext> contextAccessor,
			IEnumerable<IListenerBuilder<TContext>> listenerBuilders,
			IEnumerable<IServiceAction<TContext>> serviceActions)
		{
			ApplicationName = environment.ApplicationName;
			ContextAccessor = contextAccessor;
			ServiceAccessor = serviceAccessor;
			ListenerBuilders = listenerBuilders;
			ServiceActions = serviceActions;
		}

		public abstract Task RunServiceAsync(CancellationToken cancellationToken);
	}
}
