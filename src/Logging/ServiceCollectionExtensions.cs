// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Omex.Extensions.Abstractions.ServiceContext;

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
	}
}
