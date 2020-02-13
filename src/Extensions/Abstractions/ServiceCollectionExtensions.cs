// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary> Extension methods for the <see cref="IServiceCollection"/> class</summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>Add IServiceContext to ServiceCollection</summary>
		public static IServiceCollection AddOmexServiceContext<TServiceContext>(this IServiceCollection serviceCollection)
			where TServiceContext : class, IServiceContext
		{
			serviceCollection.TryAddTransient<IServiceContext, TServiceContext>();
			return serviceCollection;
		}


		/// <summary>Add IServiceContext without any information to ServiceCollection</summary>
		public static IServiceCollection AddEmptyOmexServiceContext(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddTransient<IServiceContext, EmptyServiceContext>();
			return serviceCollection;
		}


		/// <summary>Add IMachineInformation to ServiceCollection</summary>
		public static IServiceCollection AddOmexMachineInformation(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddTransient<IMachineInformation, BasicMachineInformation>();
			return serviceCollection;
		}
	}
}
