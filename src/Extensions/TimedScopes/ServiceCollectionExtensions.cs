// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Omex.Extensions.Abstractions.ReplayableLogs;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	/// <summary>
	/// Extension methods for the <see cref="IServiceCollection"/> class
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Add TimedScoped to ServiceCollection
		/// </summary>
		public static IServiceCollection AddTimedScopes(this IServiceCollection serviceCollection)
		{
			Activity.DefaultIdFormat = ActivityIdFormat.W3C;
			serviceCollection.TryAddTransient<IActivityProvider, SimpleActivityProvider>();
			serviceCollection.TryAddTransient<ITimedScopeProvider,TimedScopeProvider>();
			serviceCollection.TryAddTransient<ITimedScopeEventSender, TimedScopeEventSender>();
			serviceCollection.TryAddSingleton(p => TimedScopeEventSource.Instance);
			return serviceCollection;
		}
	}
}
