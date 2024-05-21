﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
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
		[Obsolete("OmexLogger and OmexLogEventSource are obsolete and pending for removal by 1 July 2024. Please consider using a different Logger.", DiagnosticId = "OMEX188")]
		public static ILoggingBuilder AddOmexLogging(this ILoggingBuilder builder)
		{
			builder.AddConfiguration();
			builder.Services.AddOmexLogging();
			return builder;
		}

		/// <summary>
		/// Adds Omex event logger to the factory
		/// </summary>
		/// <param name="serviceCollection">The extension method argument</param>
		/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained</returns>
		[Obsolete("OmexLogger and OmexLogEventSource are obsolete and pending for removal by 1 July 2024. Please consider using a different Logger.", DiagnosticId = "OMEX188")]
		public static IServiceCollection AddOmexLogging(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddLogging();

			serviceCollection.TryAddTransient<IServiceContext, EmptyServiceContext>();
			serviceCollection.TryAddTransient<IExecutionContext, BaseExecutionContext>();
			serviceCollection.TryAddTransient<IExternalScopeProvider, LoggerExternalScopeProvider>();

			serviceCollection.TryAddSingleton(_ => OmexLogEventSource.Instance);
			serviceCollection.TryAddSingleton<ILogEventReplayer, OmexLogEventReplayer>();
			serviceCollection.TryAddSingleton<ILogEventSender, OmexLogEventSender>();

			serviceCollection.TryAddEnumerable(ServiceDescriptor.Transient<IActivityStopObserver, ReplayableActivityStopObserver>());
			serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, OmexLoggerProvider>());

			serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton
				<IConfigureOptions<OmexLoggingOptions>, OmexLoggerOptionsSetup>());
			serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton
			<IOptionsChangeTokenSource<OmexLoggingOptions>,
				LoggerProviderOptionsChangeTokenSource<OmexLoggingOptions, OmexLoggerProvider>>());

			LoggerProviderOptions.RegisterProviderOptions<OmexLoggingOptions, OmexLoggerProvider>(serviceCollection);

			return serviceCollection;
		}
	}
}
