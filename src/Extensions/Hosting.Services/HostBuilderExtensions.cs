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
using Microsoft.ServiceFabric.Services.Runtime;

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
		/// <typeparam name="TService">Type of service fabric service, currently should be only <see cref="StatelessService"/> or <see cref="StatefulService"/></typeparam>
		public static ServiceFabricHostBuilder<TService> AddServiceAction<TService>(
			this ServiceFabricHostBuilder<TService> builder,
			Func<IServiceProvider, TService, CancellationToken, Task> action) =>
				builder.ConfigureServices((config, collection) =>
					collection.AddSingleton<IServiceAction<TService>>(p => new ServiceAction<TService>(p, action)));

		/// <summary>
		/// Add service listener to service fabric service
		/// </summary>
		/// <typeparam name="TService">Type of service fabric service, currently should be only <see cref="StatelessService"/> or <see cref="StatefulService"/></typeparam>
		public static ServiceFabricHostBuilder<TService> AddServiceListener<TService>(
			this ServiceFabricHostBuilder<TService> builder,
			string name,
			Func<IServiceProvider, TService, ICommunicationListener> createListener) =>
				builder.ConfigureServices((config, collection) =>
					collection.AddSingleton<IListenerBuilder<TService>>(p => new ListenerBuilder<TService>(name, p, createListener)));

		/// <summary>
		/// Add actions that will be executed inside service fabric stateless service RunAsync method
		/// </summary>
		public static ServiceFabricHostBuilder<StatelessService> AddServiceAction<TAction>(this ServiceFabricHostBuilder<StatelessService> builder)
			where TAction : class, IServiceAction<StatelessService> =>
				builder.AddServiceAction<TAction>();

		/// <summary>
		/// Add actions that will be executed inside service fabric stateful service RunAsync method
		/// </summary>
		public static ServiceFabricHostBuilder<StatefulService> AddServiceAction<TAction>(this ServiceFabricHostBuilder<StatefulService> builder)
			where TAction : class, IServiceAction<StatefulService> =>
				builder.AddServiceAction<TAction>();

		/// <summary>
		/// Add service listener to stateful service fabric service
		/// </summary>
		public static ServiceFabricHostBuilder<StatefulService> AddServiceListener<TListener>(this ServiceFabricHostBuilder<StatefulService> builder)
			where TListener : class, IListenerBuilder<StatefulService> =>
				builder.AddServiceListener<TListener>();

		/// <summary>
		/// Add service listener to stateless service fabric service
		/// </summary>
		public static ServiceFabricHostBuilder<StatelessService> AddServiceListener<TListener>(this ServiceFabricHostBuilder<StatelessService> builder)
			where TListener : class, IListenerBuilder<StatelessService> =>
				builder.AddServiceListener<TListener>();

		/// <summary>
		/// Configures host to run service fabric stateless service with initializded Omex dependencies
		/// </summary>
		public static IHost BuildStatelessService(
			this IHostBuilder builder,
			string serviceName,
			Action<ServiceFabricHostBuilder<StatelessService>> builderAction) =>
				builder.BuildServiceFabricService<OmexStatelessServiceRunner, StatelessService, StatelessServiceContext>(serviceName, builderAction);

		/// <summary>
		/// Configures host to run service fabric stateful service with initializded Omex dependencies
		/// </summary>
		public static IHost BuildStatefulService(
			this IHostBuilder builder,
			string serviceName,
			Action<ServiceFabricHostBuilder<StatefulService>> builderAction) =>
				builder.BuildServiceFabricService<OmexStatefulServiceRunner, StatefulService, StatefulServiceContext>(serviceName, builderAction);

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

		private static ServiceFabricHostBuilder<TService> AddServiceAction<TService, TAction>(this ServiceFabricHostBuilder<TService> builder)
			where TAction : class, IServiceAction<TService> =>
				builder.ConfigureServices((config, collection) => collection.AddTransient<IServiceAction<TService>, TAction>());

		private static ServiceFabricHostBuilder<TService> AddServiceListener<TService, TListener>(this ServiceFabricHostBuilder<TService> builder)
			where TListener : class, IListenerBuilder<TService> =>
				builder.ConfigureServices((config, collection) => collection.AddTransient<IListenerBuilder<TService>, TListener>());

		private static IHost BuildServiceFabricService<TRunner, TService, TContext>(
			this IHostBuilder builder,
			string serviceName,
			Action<ServiceFabricHostBuilder<TService>> builderAction)
				where TContext : ServiceContext
				where TRunner : class, IOmexServiceRunner
		{
			try
			{
				builderAction(new ServiceFabricHostBuilder<TService>(builder));

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

				ServiceInitializationEventSource.Instance.LogServiceTypeRegistered(Process.GetCurrentProcess().Id, serviceName);

				return host;
			}
			catch (Exception e)
			{
				ServiceInitializationEventSource.Instance.LogServiceHostInitializationFailed(e.ToString(), serviceName);
				throw;
			}
		}

		private static IHostBuilder UseApplicationName(this IHostBuilder builder, string applicationName) =>
			builder.ConfigureAppConfiguration(configuration =>
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
