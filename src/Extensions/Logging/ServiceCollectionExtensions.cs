// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Logging.Replayable;
using Microsoft.Omex.Extensions.TimedScopes;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Extension methods for the <see cref="ILoggerFactory"/> class
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
			serviceCollection
				.AddLogging()
				.Configure<OmexLoggingOptions>(options =>
				{
					options.ReplayLogsInCaseOfError = false;
				});

			serviceCollection.TryAddTransient<IServiceContext, EmptyServiceContext>();
			serviceCollection.TryAddTransient<IMachineInformation, BasicMachineInformation>();
			serviceCollection.TryAddTransient<IExternalScopeProvider, LoggerExternalScopeProvider>();

			serviceCollection.TryAddTransient<IActivityProvider, ReplayableActivityProvider>();

			serviceCollection.TryAddSingleton(p => OmexLogEventSource.Instance);
			serviceCollection.TryAddTransient<ILogEventReplayer, OmexLogEventSender>();
			serviceCollection.TryAddTransient<ILogEventSender, OmexLogEventSender>();

			serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, OmexLoggerProvider>());
			return serviceCollection;
		}
	}
}
