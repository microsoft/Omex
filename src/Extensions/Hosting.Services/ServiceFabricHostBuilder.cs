// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Wrapper on top of IHostBuilder to propagete proper context type and avoid type registration mistakes
	/// </summary>
	public sealed class ServiceFabricHostBuilder<TService, TContext>
		where TService : IServiceFabricService<TContext>
		where TContext : ServiceContext
	{
		private readonly IHostBuilder m_builder;

		internal ServiceFabricHostBuilder(IHostBuilder builder) => m_builder = builder;

		/// <summary>
		/// Method should be called only by extensions of this class
		/// If you are creating a service please register dependencies before calling BuildService method and using this class
		/// </summary>
		internal ServiceFabricHostBuilder<TService, TContext> ConfigureServices(Action<HostBuilderContext, IServiceCollection> action)
		{
			m_builder.ConfigureServices(action);
			return this;
		}

		/// <summary>
		/// Add actions that will be executed inside service fabric service RunAsync method
		/// </summary>
		public ServiceFabricHostBuilder<TService, TContext> AddServiceAction(Func<IServiceProvider, CancellationToken, Task> action) =>
			ConfigureServices((config, collection) => collection.AddSingleton<IServiceAction<TContext>>(p => new ServiceAction<TContext>(p, action)));

		/// <summary>
		/// Add actions that will be executed inside service fabric service RunAsync method
		/// </summary>
		public ServiceFabricHostBuilder<TService, TContext> AddServiceAction(Func<IServiceProvider, IServiceAction<TContext>> implementationFactory) =>
			ConfigureServices((config, collection) => collection.AddSingleton(implementationFactory));

		/// <summary>
		/// Add service listener to service fabric service
		/// </summary>
		public ServiceFabricHostBuilder<TService, TContext> AddServiceListener(string name, Func<IServiceProvider, TContext, ICommunicationListener> createListener) =>
			ConfigureServices((config, collection) => collection.AddSingleton<IListenerBuilder<TContext>>(p => new ListenerBuilder<TContext>(name, p, createListener)));

		/// <summary>
		/// Add service listener to service fabric service
		/// </summary>
		public ServiceFabricHostBuilder<TService, TContext> AddServiceListener(Func<IServiceProvider, IListenerBuilder<TContext>> implementationFactory) =>
			ConfigureServices((config, collection) => collection.AddSingleton(implementationFactory));

		/// <summary>
		/// Add actions that will be executed inside service fabric service RunAsync method
		/// </summary>
		public ServiceFabricHostBuilder<TService, TContext> AddServiceAction<TAction>()
			where TAction : class, IServiceAction<TContext> =>
				ConfigureServices((config, collection) => collection.AddTransient<IServiceAction<TContext>, TAction>());

		/// <summary>
		/// Add service listener to stateless service fabric service
		/// </summary>
		public ServiceFabricHostBuilder<TService, TContext> AddServiceListener<TListener>()
			where TListener : class, IListenerBuilder<TContext> =>
				ConfigureServices((config, collection) => collection.AddTransient<IListenerBuilder<TContext>, TListener>());
	}
}
