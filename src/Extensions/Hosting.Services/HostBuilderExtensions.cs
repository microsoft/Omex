// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Fabric;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.ServiceFabric.Data;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Extension to add Omex dependencies to HostBuilder
	/// </summary>
	public static class HostBuilderExtensions
	{
		/// <summary>
		/// Configures host to run service fabric stateless service with initialized Omex dependencies
		/// </summary>
		public static IHost BuildStatelessService(
			this IHostBuilder builder,
			string serviceName,
			Action<ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>> builderAction) =>
				builder.BuildServiceFabricService<OmexStatelessServiceRegistrator, OmexStatelessService, StatelessServiceContext>(serviceName, builderAction);

		/// <summary>
		/// Configures host to run service fabric stateful service with initialized Omex dependencies
		/// </summary>
		public static IHost BuildStatefulService(
			this IHostBuilder builder,
			string serviceName,
			Action<ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext>> builderAction) =>
				builder.BuildServiceFabricService<OmexStatefulServiceRegistrator, OmexStatefulService, StatefulServiceContext>(serviceName, builderAction);

		/// <summary>
		/// Registering Dependency Injection classes that will provide Service Fabric specific information for logging
		/// </summary>
		public static IServiceCollection AddOmexServiceFabricDependencies<TContext>(this IServiceCollection collection)
			where TContext : ServiceContext
		{
			bool isStatefulService = typeof(StatefulServiceContext).IsAssignableFrom(typeof(TContext));

			if (isStatefulService)
			{
				collection.TryAddAccessor<IReliableStateManager>();
				collection.TryAddAccessor<IStatefulServicePartition, IServicePartition>();
			}
			else
			{
				collection.TryAddAccessor<IStatelessServicePartition, IServicePartition>();
			}

			collection.TryAddAccessor<TContext, ServiceContext>();

			collection.TryAddSingleton<IServiceContext, OmexServiceFabricContext>();
			collection.TryAddSingleton<IExecutionContext, ServiceFabricExecutionContext>();
			return collection.AddOmexServices();
		}

		private static IHost BuildServiceFabricService<TRunner, TService, TContext>(
			this IHostBuilder builder,
			string serviceName,
			Action<ServiceFabricHostBuilder<TService, TContext>> builderAction)
				where TRunner : OmexServiceRegistrator<TService, TContext>
				where TService : IServiceFabricService<TContext>
				where TContext : ServiceContext
		{
			Validation.ThrowIfNullOrWhiteSpace(serviceName, nameof(serviceName));

			// for generic host application name is the name of the service that it's running (don't confuse with Sf application name)
			builder.UseApplicationName(serviceName);

			builderAction(new ServiceFabricHostBuilder<TService, TContext>(builder));

			return builder
				.ConfigureServices((context, collection) =>
				{
					collection
						.AddOmexServiceFabricDependencies<TContext>()
						.AddSingleton<IOmexServiceRegistrator, TRunner>()
						.AddHostedService<OmexHostedService>();
				})
				.BuildWithErrorReporting();
		}

		/// <summary>
		/// Overrides ApplicationName in host configuration
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

		internal static IServiceCollection TryAddAccessor<TValue, TBase>(this IServiceCollection collection)
			where TValue : class, TBase
			where TBase : class
		{
			collection.TryAddAccessor<TValue>();
			collection.TryAddSingleton<IAccessor<TBase>>(p => p.GetRequiredService<Accessor<TValue>>());
			return collection;
		}

		internal static IServiceCollection TryAddAccessor<TValue>(this IServiceCollection collection)
			where TValue : class
		{
			collection.TryAddSingleton<Accessor<TValue>, Accessor<TValue>>();
			collection.TryAddSingleton<IAccessorSetter<TValue>>(p => p.GetService<Accessor<TValue>>());
			collection.TryAddSingleton<IAccessor<TValue>>(p => p.GetService<Accessor<TValue>>());
			return collection;
		}
	}
}
