// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary> Extension methods for the <see cref="IServiceCollection"/> class</summary>
	public static class OmexServiceCollectionExtensions
	{
		/// <summary>Add IServiceContext to ServiceCollection</summary>
		public static IServiceCollection AddServiceContext<TServiceContext>(this IServiceCollection serviceCollection)
			where TServiceContext : class, IServiceContext
		{
			serviceCollection.TryAddSingleton<IServiceContext, TServiceContext>();
			return serviceCollection;
		}


		/// <summary>Add Null implementation of IServiceContext to ServiceCollection</summary>
		public static IServiceCollection AddNullServiceContext(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddSingleton<IServiceContext, NullServiceContext>();
			return serviceCollection;
		}


		/// <summary>Add IMachineInformation to ServiceCollection</summary>
		public static IServiceCollection AddMachineInformation(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddSingleton<IMachineInformation, BasicMachineInformation>();
			return serviceCollection;
		}
	}
}
