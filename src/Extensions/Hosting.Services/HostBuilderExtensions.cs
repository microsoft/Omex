// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Fabric;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Extension to add Omex dependencies to HostBuilder
	/// </summary>
	public static class HostBuilderExtensions
	{
		/// <summary>Add actions that will be executed inside service fabric service RunAsync method</summary>
		public static ServiceFabricHostBuilder<TContext> AddServiceAction<TContext>(
			this ServiceFabricHostBuilder<TContext> builder,
			Func<TContext, CancellationToken, Task> action)
				where TContext : ServiceContext =>
			builder.AddServiceAction(new ServiceAction<TContext>(action));


		/// <summary>Add actions that will be executed inside stateless service RunAsync method</summary>
		public static ServiceFabricHostBuilder<TContext> AddServiceAction<TContext>(
			this ServiceFabricHostBuilder<TContext> builder,
			IServiceAction<TContext> action)
				where TContext : ServiceContext =>
			builder.ConfigureServices((config, collection) => collection.AddSingleton(action));


		/// <summary>Add service listener to stateless service fabric service</summary>
		public static ServiceFabricHostBuilder<TContext> AddServiceListener<TContext>(
			this ServiceFabricHostBuilder<TContext> builder,
			string name,
			Func<TContext, ICommunicationListener> createListener)
				where TContext : ServiceContext =>
			builder.AddServiceListener(new ListenerBuilder<TContext>(name, createListener));


		/// <summary>Add service listener to stateless service fabric service</summary>
		public static ServiceFabricHostBuilder<TContext> AddServiceListener<TContext>(
			this ServiceFabricHostBuilder<TContext> builder,
			IListenerBuilder<TContext> listenerBuilder)
				where TContext : ServiceContext =>
			builder.ConfigureServices((config, collection) => collection.AddSingleton(listenerBuilder));


		/// <summary>Configures host to run service fabric stateless service with initializded Omex dependencies</summary>
		public static IHost BuildStatelessService(
			this IHostBuilder builder,
			Action<ServiceFabricHostBuilder<StatelessServiceContext>> builderAction) =>
			builder.BuildServiceFabricService<OmexStatelessServiceRunner, StatelessServiceContext>(builderAction);


		/// <summary>Configures host to run service fabric statefull service with initializded Omex dependencies</summary>
		public static IHost BuildStatefullService(
			this IHostBuilder builder,
			Action<ServiceFabricHostBuilder<StatefulServiceContext>> builderAction) =>
			builder.BuildServiceFabricService<OmexStatefulServiceRunner, StatefulServiceContext>(builderAction);


		/// <summary> Registerin DI classes that will provide Serfice Fabric specific information for logging </summary>
		public static IServiceCollection AddOmexServiceFabricDependencies<TContext>(this IServiceCollection collection)
			where TContext : ServiceContext
		{
			collection.TryAddSingleton<ServiceContextAccessor<TContext>, ServiceContextAccessor<TContext>>();
			collection.TryAddSingleton<IServiceContextAccessor<TContext>>(p => p.GetService<ServiceContextAccessor<TContext>>());
			collection.TryAddSingleton<IServiceContextAccessor<ServiceContext>>(p => p.GetService<ServiceContextAccessor<TContext>>());
			collection.TryAddSingleton<IServiceContext, OmexServiceFabricContext>();
			collection.TryAddSingleton<IMachineInformation, ServiceFabricMachineInformation>();
			return collection.AddOmexServices();
		}


		private static IHost BuildServiceFabricService<TRunner,TContext>(
			this IHostBuilder builder,
			Action<ServiceFabricHostBuilder<TContext>> builderAction)
				where TContext : ServiceContext
				where TRunner : class, IOmexServiceRunner
		{
			try
			{
				builderAction(new ServiceFabricHostBuilder<TContext>(builder));

				IHost host = builder
					.ConfigureServices((context, collection) =>
					{
						collection
							.AddOmexServiceFabricDependencies<TContext>()
							.AddTransient<IHostedService, OmexHostedService>()
							.AddSingleton<IOmexServiceRunner, TRunner>();
					})
					.UseDefaultServiceProvider(options =>
					{
						options.ValidateOnBuild = true;
						options.ValidateScopes = true;
					})
					.Build();

				string m_applicationName = Assembly.GetExecutingAssembly().GetName().FullName;
				ServiceInitializationEventSource.Instance.LogServiceTypeRegistered(Process.GetCurrentProcess().Id, m_applicationName);

				return host;
			}
			catch (Exception e)
			{
				ServiceInitializationEventSource.Instance.LogServiceHostInitializationFailed(e.ToString());
				throw;
			}
		}
	}
}
