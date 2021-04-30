// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions
{
	/// <summary>
	/// Extension to add Service Fabric Client dependencies to IServiceCollection
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Register types required Service Fabric comunication
		/// </summary>
		public static IServiceCollection AddServiceFabricClient(this IServiceCollection serviceCollection)
		{
			return serviceCollection.AddServiceFabricClient<ServiceFabricClientWrapper>();
		}

		/// <summary>
		/// Register generic types required Service Fabric comunication
		/// </summary>
		public static IServiceCollection AddServiceFabricClient<TClinet>(this IServiceCollection serviceCollection)
			where TClinet : class, IServiceFabricClientWrapper
		{
			serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IServiceFabricClientWrapper, TClinet>());
			return serviceCollection;
		}
	}
}
