// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.Omex.Extensions.Activities;
using Microsoft.Omex.Extensions.Activities.Option;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for the <see cref="IServiceCollection"/> class
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		private const string ActivitySourceName = "OmexActivitySource";
		private const string ActivitySourceVersion = "1.0.0.0";

		/// <summary>
		/// Add ActivitySource to ServiceCollection
		/// </summary>
		public static IServiceCollection AddOmexActivitySource(this IServiceCollection serviceCollection)
		{
			Activity.DefaultIdFormat = ActivityIdFormat.W3C;
			Activity.ForceDefaultIdFormat = true;
			serviceCollection.AddHostedService<ActivityListenerInitializerService>();
			serviceCollection.AddHostedService<DiagnosticsObserversInitializer>();

			serviceCollection.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
			serviceCollection.TryAddSingleton<ActivityObserver>();
			serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IActivityStartObserver, ActivityObserver>(p => p.GetRequiredService<ActivityObserver>()));
			serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IActivityStopObserver, ActivityObserver>(p => p.GetRequiredService<ActivityObserver>()));

			serviceCollection.TryAddSingleton<IExecutionContext, BaseExecutionContext>();

			// eventually ActivityMetricsSender will be default implementation of IActivitiesEventSender and we should remove ActivityEventSender, and AggregatedActivitiesEventSender
			serviceCollection.TryAddSingleton<ActivityMetricsSender>();
			serviceCollection.TryAddSingleton<ActivityEventSender>();
			serviceCollection.TryAddSingleton<IActivitiesEventSender, AggregatedActivitiesEventSender>();

			serviceCollection
				.AddOptions<ActivityOption>()
				.BindConfiguration(ActivityOption.MonitoringPath)
				.ValidateDataAnnotations();

			serviceCollection.TryAddSingleton<IActivityListenerConfigurator, DefaultActivityListenerConfigurator>();
			serviceCollection.TryAddSingleton(p => new ActivitySource(ActivitySourceName, ActivitySourceVersion));
			serviceCollection.TryAddSingleton(p => ActivityEventSource.Instance);

			return serviceCollection;
		}
	}
}
