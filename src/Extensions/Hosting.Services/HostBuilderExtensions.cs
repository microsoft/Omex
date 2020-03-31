// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
		/// <summary>
		/// Add actions that will be executed inside service fabric service RunAsync method
		/// </summary>
		public static ServiceFabricHostBuilder<TService, TContext> AddServiceAction<TService, TContext>(
			this ServiceFabricHostBuilder<TService, TContext> builder,
			Func<IServiceProvider, TService, CancellationToken, Task> action)
				where TService : IServiceFabricService<TContext>
				where TContext : ServiceContext =>
					builder.ConfigureServices((config, collection) =>
						collection.AddSingleton<IServiceAction<TService>>(p =>
							new ServiceAction<TService, TContext>(p, action)));

		/// <summary>
		/// Add actions that will be executed inside service fabric service RunAsync method
		/// </summary>
		public static ServiceFabricHostBuilder<TService, TContext> AddServiceAction<TService, TContext>(
			this ServiceFabricHostBuilder<TService, TContext> builder,
			Func<IServiceProvider, IServiceAction<TService>> implementationFactory)
				where TService : IServiceFabricService<TContext>
				where TContext : ServiceContext =>
					builder.ConfigureServices((config, collection) =>
						collection.AddSingleton(implementationFactory));

		/// <summary>
		/// Add service listener to service fabric service
		/// </summary>
		public static ServiceFabricHostBuilder<TService, TContext> AddServiceListener<TService, TContext>(
			this ServiceFabricHostBuilder<TService, TContext> builder,
			string name,
			Func<IServiceProvider, TService, ICommunicationListener> createListener)
				where TService : IServiceFabricService<TContext>
				where TContext : ServiceContext =>
				builder.ConfigureServices((config, collection) =>
					collection.AddSingleton<IListenerBuilder<TService>>(p =>
						new ListenerBuilder<TService, TContext>(name, p, createListener)));

		/// <summary>
		/// Add service listener to service fabric service
		/// </summary>
		public static ServiceFabricHostBuilder<TService, TContext> AddServiceListener<TService, TContext>(
			this ServiceFabricHostBuilder<TService, TContext> builder,
			Func<IServiceProvider, IListenerBuilder<TService>> implementationFactory)
				where TService : IServiceFabricService<TContext>
				where TContext : ServiceContext =>
					builder.ConfigureServices((config, collection) =>
						collection.AddSingleton(implementationFactory));

		/// <summary>
		/// Add actions that will be executed inside service fabric stateless service RunAsync method
		/// </summary>
		public static ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> AddServiceAction<TAction>(
			this ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> builder)
				where TAction : class, IServiceAction<OmexStatelessService> =>
					builder.AddServiceAction<TAction, OmexStatelessService, StatelessServiceContext>();

		/// <summary>
		/// Add actions that will be executed inside service fabric stateful service RunAsync method
		/// </summary>
		public static ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> AddServiceAction<TAction>(
			this ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> builder)
				where TAction : class, IServiceAction<OmexStatefulService> =>
					builder.AddServiceAction<TAction, OmexStatefulService, StatefulServiceContext>();

		/// <summary>
		/// Add service listener to stateful service fabric service
		/// </summary>
		public static ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> AddServiceListener<TListener>(
			this ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> builder)
				where TListener : class, IListenerBuilder<OmexStatefulService> =>
					builder.AddServiceListener<TListener, OmexStatefulService, StatefulServiceContext>();

		/// <summary>
		/// Add service listener to stateless service fabric service
		/// </summary>
		public static ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> AddServiceListener<TListener>(
			this ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> builder)
				where TListener : class, IListenerBuilder<OmexStatelessService> =>
					builder.AddServiceListener<TListener, OmexStatelessService, StatelessServiceContext>();

		/// <summary>
		/// Add actions that will be executed inside service fabric service RunAsync method
		/// </summary>
		public static ServiceFabricHostBuilder<TService, TContext> AddServiceAction<TAction, TService, TContext>(this ServiceFabricHostBuilder<TService, TContext> builder)
			where TAction : class, IServiceAction<TService>
			where TService : IServiceFabricService<TContext>
			where TContext : ServiceContext =>
				builder.ConfigureServices((config, collection) => collection.AddTransient<IServiceAction<TService>, TAction>());

		/// <summary>
		/// Add service listener to stateless service fabric service
		/// </summary>
		public static ServiceFabricHostBuilder<TService, TContext> AddServiceListener<TListener, TService, TContext>(this ServiceFabricHostBuilder<TService, TContext> builder)
			where TListener : class, IListenerBuilder<TService>
			where TService : IServiceFabricService<TContext>
			where TContext : ServiceContext =>
				builder.ConfigureServices((config, collection) => collection.AddTransient<IListenerBuilder<TService>, TListener>());

		/// <summary>
		/// Configures host to run service fabric stateless service with initializded Omex dependencies
		/// </summary>
		public static IHost BuildStatelessService(
			this IHostBuilder builder,
			string serviceName,
			Action<ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>> builderAction) =>
				builder.BuildServiceFabricService<OmexStatelessServiceRunner, OmexStatelessService, StatelessServiceContext>(serviceName, builderAction);

		/// <summary>
		/// Configures host to run service fabric stateful service with initializded Omex dependencies
		/// </summary>
		public static IHost BuildStatefulService(
			this IHostBuilder builder,
			string serviceName,
			Action<ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext>> builderAction) =>
				builder.BuildServiceFabricService<OmexStatefulServiceRunner, OmexStatefulService, StatefulServiceContext>(serviceName, builderAction);

		/// <summary>
		/// Registering Dependency Injection classes that will provide Service Fabric specific information for logging
		/// </summary>
		public static IServiceCollection AddOmexServiceFabricDependencies<TContext>(this IServiceCollection collection)
			where TContext : ServiceContext
		{
			collection.TryAddSingleton<ServiceContextAccessor<TContext>, ServiceContextAccessor<TContext>>();
			collection.TryAddSingleton<IServiceContextAccessor<TContext>>(p => p.GetService<ServiceContextAccessor<TContext>>());
			collection.TryAddSingleton<IServiceContextAccessor<ServiceContext>>(p => p.GetService<ServiceContextAccessor<TContext>>());
			collection.TryAddSingleton<IServiceContext, OmexServiceFabricContext>();
			collection.TryAddSingleton<IExecutionContext, ServiceFabricExecutionContext>();
			return collection.AddOmexServices();
		}

		private static IHost BuildServiceFabricService<TRunner, TService, TContext>(
			this IHostBuilder builder,
			string serviceName,
			Action<ServiceFabricHostBuilder<TService, TContext>> builderAction)
				where TRunner : OmexServiceRunner<TService, TContext>
				where TService : IServiceFabricService<TContext>
				where TContext : ServiceContext
		{
			try
			{
				builderAction(new ServiceFabricHostBuilder<TService, TContext>(builder));

				if (string.IsNullOrWhiteSpace(serviceName))
				{
					// use executing asembly name for loggins since application name might be not available yet
					serviceName = Assembly.GetExecutingAssembly().GetName().FullName;
				}
				else
				{
					// override default application name if it's provided explisitly
					// for generic host application name is the name of the service that it's running (don't confuse with Sf application name)
					builder.UseApplicationName(serviceName);
				}

				IHost host = builder
					.ConfigureServices((context, collection) =>
					{
						collection
							.AddOmexServiceFabricDependencies<TContext>()
							.AddSingleton<IOmexServiceRunner, TRunner>()
							.AddHostedService<OmexHostedService>();
					})
					.UseDefaultServiceProvider(options =>
					{
						options.ValidateOnBuild = true;
						options.ValidateScopes = true;
					})
					.Build();

				// get proper application name from host
				serviceName = host.Services.GetService<IHostEnvironment>().ApplicationName;

				ServiceInitializationEventSource.Instance.LogHostBuildeSucceded(Process.GetCurrentProcess().Id, serviceName);

				return host;
			}
			catch (Exception e)
			{
				ServiceInitializationEventSource.Instance.LogHostBuildFailed(e.ToString(), serviceName);
				throw;
			}
		}

		/// <summary>
		/// Overides ApplicationName in host configuration
		/// </summary>
		/// <remarks>
		/// Method done internal instead of private to create unit tests for it,
		/// since failure to set proper application name could cause Service Fabric error that is hard to debug:
		///    System.Fabric.FabricException: Invalid Service Type
		/// </remarks>
		internal static IHostBuilder UseApplicationName(this IHostBuilder builder, string applicationName) =>
			builder.ConfigureHostConfiguration(configuration =>
			{
				configuration.AddInMemoryCollection(new[]
				{
					new KeyValuePair<string, string>(
						HostDefaults.ApplicationKey,
						string.IsNullOrWhiteSpace(applicationName)
							? throw new ArgumentNullException(nameof(applicationName))
							: applicationName)
				});
			});
	}
}
