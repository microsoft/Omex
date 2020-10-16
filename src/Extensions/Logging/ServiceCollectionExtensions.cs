﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.Logging.Replayable;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Extension methods for the <see cref="IServiceCollection"/> class
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Add IServiceContext to ServiceCollection
		/// </summary>
		public static IServiceCollection AddOmexServiceContext<TServiceContext>(this IServiceCollection serviceCollection)
			where TServiceContext : class, IServiceContext
		{
			serviceCollection.TryAddTransient<IServiceContext, TServiceContext>();
			return serviceCollection;
		}

		/// <summary>
		/// Adds Omex event logger to the factory
		/// </summary>
		/// <param name="builder">The extension method argument</param>
		public static ILoggingBuilder AddOmexLogging(this ILoggingBuilder builder)
		{
			builder.Services.AddOmexLogging();
			return builder;
		}

		/// <summary>
		/// Adds Omex event logger to the factory
		/// </summary>
		/// <param name="serviceCollection">The extension method argument</param>
		/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained</returns>
		public static IServiceCollection AddOmexLogging(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddLogging();

			serviceCollection.TryAddTransient<IServiceContext, EmptyServiceContext>();
			serviceCollection.TryAddTransient<IExecutionContext, BasicMachineInformation>();
			serviceCollection.TryAddTransient<IExternalScopeProvider, LoggerExternalScopeProvider>();
			serviceCollection.TryAddTransient<IActivityProvider, ReplayableActivityProvider>();

			serviceCollection.TryAddSingleton(p => OmexLogEventSource.Instance);
			serviceCollection.TryAddTransient<ILogEventReplayer, OmexLogEventSender>();
			serviceCollection.TryAddTransient<ILogEventSender, OmexLogEventSender>();

			serviceCollection.TryAddEnumerable(ServiceDescriptor.Transient<IActivityStopObserver, ReplayableActivityStopObserver>());
			serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, OmexLoggerProvider>());

			return serviceCollection;
		}
	}
}
