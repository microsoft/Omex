// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Extension to add Omex dependencies to HostBuilder
	/// </summary>
	public static class HostBuilderExtensions
	{
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

				ServiceInitializationEventSource.Instance.LogHostBuildSucceeded(Process.GetCurrentProcess().Id, serviceName);

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
