// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Accessors;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Abstract class for registering Omex services.
	/// </summary>
	/// <typeparam name="TService">The type of the service.</typeparam>
	/// <typeparam name="TContext">The type of the service context.</typeparam>
	public abstract class OmexServiceRegistrator<TService, TContext> : IOmexServiceRegistrator
		where TService : IServiceFabricService<TContext>
		where TContext : ServiceContext
	{
		/// <summary>
		/// Gets the options for the service registrator.
		/// </summary>
		protected readonly ServiceRegistratorOptions Options;

		/// <summary>
		/// Gets the context accessor.
		/// </summary>
		public IAccessorSetter<TContext> ContextAccessor { get; }

		/// <summary>
		/// Gets the listener builders for the service.
		/// </summary>
		public IEnumerable<IListenerBuilder<TService>> ListenerBuilders { get; }

		/// <summary>
		/// Gets the service actions for the service.
		/// </summary>
		public IEnumerable<IServiceAction<TService>> ServiceActions { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="OmexServiceRegistrator{TService, TContext}"/> class.
		/// </summary>
		/// <param name="options">The options for the service registrator.</param>
		/// <param name="contextAccessor">The context accessor.</param>
		/// <param name="listenerBuilders">The listener builders for the service.</param>
		/// <param name="serviceActions">The service actions for the service.</param>
		public OmexServiceRegistrator(
			IOptions<ServiceRegistratorOptions> options,
			IAccessorSetter<TContext> contextAccessor,
			IEnumerable<IListenerBuilder<TService>> listenerBuilders,
			IEnumerable<IServiceAction<TService>> serviceActions)
		{
			Options = options.Value;
			ContextAccessor = contextAccessor;
			ListenerBuilders = listenerBuilders;
			ServiceActions = serviceActions;
		}

		/// <summary>
		/// Registers the service asynchronously.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A task that represents the asynchronous operation.</returns>
		public abstract Task RegisterAsync(CancellationToken cancellationToken);
	}
}
