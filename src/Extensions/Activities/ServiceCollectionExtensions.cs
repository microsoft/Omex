﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.Omex.Extensions.Activities;

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
			serviceCollection.AddHostedService<ActivityListenerInitializerService>();
			serviceCollection.AddHostedService<DiagnosticsObserversInitializer>();
			serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IActivityStopObserver, ActivityStopObserver>());

			serviceCollection.TryAddSingleton<IExecutionContext, BaseExecutionContext>();
			serviceCollection.TryAddSingleton<IActivitiesEventSender, ActivityEventSender>();
			serviceCollection.TryAddSingleton<IActivityListenerConfigurator, DefaultActivityListenerConfigurator>();
			serviceCollection.TryAddSingleton(p => new ActivitySource(ActivitySourceName, ActivitySourceVersion));
			serviceCollection.TryAddSingleton(p => ActivityEventSource.Instance);
			return serviceCollection;
		}
	}
}
