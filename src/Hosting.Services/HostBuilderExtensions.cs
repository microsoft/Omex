// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
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
			string serviceNameForLogging = serviceName;

			try
			{
				if (string.IsNullOrWhiteSpace(serviceName))
				{
					// use executing assembly name for logging since application name not available
					serviceNameForLogging = Assembly.GetExecutingAssembly().GetName().FullName;
					throw new ArgumentException("Service type name is null of whitespace", nameof(serviceName));
				}

				builderAction(new ServiceFabricHostBuilder<TService, TContext>(builder));

				IHost host = builder
					.ConfigureServices((context, collection) =>
					{
						collection.AddOmexServiceFabricDependencies<TContext>();

						collection
							.Configure<ServiceRegistratorOptions>(options =>
							{
								options.ServiceTypeName = serviceName;
							})
							.AddSingleton<IOmexServiceRegistrator, TRunner>()
							.AddHostedService<OmexHostedService>();
					})
					.UseDefaultServiceProvider(options =>
					{
						options.ValidateOnBuild = true;
						options.ValidateScopes = true;
					})
					.Build();

#pragma warning disable OMEX188 // InitializationLogger using OmexLogger is obsolete. DiagnosticId = "OMEX188"
				InitializationLogger.LogInitializationSucceed(serviceNameForLogging);
#pragma warning restore OMEX188 // InitializationLogger using OmexLogger is obsolete. DiagnosticId = "OMEX188"

				return host;
			}
			catch (Exception e)
			{
#pragma warning disable OMEX188 // InitializationLogger using OmexLogger is obsolete. DiagnosticId = "OMEX188"
				InitializationLogger.LogInitializationFail(serviceNameForLogging, e);
#pragma warning restore OMEX188 // InitializationLogger using OmexLogger is obsolete. DiagnosticId = "OMEX188"

				throw;
			}
		}

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
			collection.TryAddSingleton<IAccessorSetter<TValue>>(p => p.GetRequiredService<Accessor<TValue>>());
			collection.TryAddSingleton<IAccessor<TValue>>(p => p.GetRequiredService<Accessor<TValue>>());
			return collection;
		}
	}
}
