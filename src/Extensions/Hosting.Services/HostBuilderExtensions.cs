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
		public static ServiceFabricHostBuilder<TService> AddServiceAction<TService>(
			this ServiceFabricHostBuilder<TService> builder,
			Func<TService,CancellationToken, Task> action) =>
				builder.AddServiceAction(new ServiceAction<TService>(action));

		/// <summary>
		/// Add actions that will be executed inside stateless service RunAsync method
		/// </summary>
		public static ServiceFabricHostBuilder<TService> AddServiceAction<TService>(
			this ServiceFabricHostBuilder<TService> builder,
			IServiceAction<TService> action) =>
				builder.ConfigureServices((config, collection) => collection.AddSingleton(action));

		/// <summary>
		/// Add service listener to stateless service fabric service
		/// </summary>
		public static ServiceFabricHostBuilder<TService> AddServiceListener<TService>(
			this ServiceFabricHostBuilder<TService> builder,
			string name,
			Func<TService, ICommunicationListener> createListener) =>
				builder.AddServiceListener(new ListenerBuilder<TService>(name, createListener));

		/// <summary>
		/// Add service listener to stateless service fabric service
		/// </summary>
		public static ServiceFabricHostBuilder<TService> AddServiceListener<TService>(
			this ServiceFabricHostBuilder<TService> builder,
			IListenerBuilder<TService> listenerBuilder) =>
				builder.ConfigureServices((config, collection) => collection.AddSingleton(listenerBuilder));

		/// <summary>
		/// Configures host to run service fabric stateless service with initializded Omex dependencies
		/// </summary>
		public static IHost BuildStatelessService(
			this IHostBuilder builder,
			string serviceName,
			Action<ServiceFabricHostBuilder<StatelessService>> builderAction) =>
				builder.BuildServiceFabricService<OmexStatelessServiceRunner,StatelessService,StatelessServiceContext>(serviceName, builderAction);

		/// <summary>
		/// Configures host to run service fabric stateful service with initializded Omex dependencies
		/// </summary>
		public static IHost BuildStatefulService(
			this IHostBuilder builder,
			string serviceName,
			Action<ServiceFabricHostBuilder<StatefulService>> builderAction) =>
				builder.BuildServiceFabricService<OmexStatefulServiceRunner,StatefulService,StatefulServiceContext>(serviceName, builderAction);

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

		private static IHost BuildServiceFabricService<TRunner,TService,TContext>(
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
