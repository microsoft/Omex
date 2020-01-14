// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Logging.TimedScopes;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	/// <summary> Extension methods for the <see cref="IServiceCollection"/> class</summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>Add IServiceContext to ServiceCollection</summary>
		public static IServiceCollection AddTimedScopes(this IServiceCollection serviceCollection)
		{
			Activity.DefaultIdFormat = ActivityIdFormat.W3C;
			serviceCollection.AddOmexMachineInformation();
			serviceCollection.TryAddTransient<IActivityProvider, SimpleActivityProvider>();
			serviceCollection.TryAddTransient<ITimedScopeProvider,TimedScopeProvider>();
			serviceCollection.TryAddTransient<TimedScopeEventSource, TimedScopeEventSource>();
			return serviceCollection;
		}
	}
}
