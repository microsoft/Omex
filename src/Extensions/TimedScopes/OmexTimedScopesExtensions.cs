// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.Omex.Extensions.Logging.TimedScopes;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary> Extension methods for the <see cref="IServiceCollection"/> class</summary>
	public static class OmexTimedScopesExtensions
	{
		/// <summary>Add IServiceContext to ServiceCollection</summary>
		public static IServiceCollection AddTimedScopes(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddMachineInformation();
			serviceCollection.TryAddSingleton<TimedScopeEventSource>();
			serviceCollection.TryAddSingleton<ITimedScopeProvider,TimedScopeProvider>();
			return serviceCollection;
		}
	}
}
