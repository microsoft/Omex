// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Omex.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary> Extension methods for the <see cref="IServiceCollection"/> class</summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary> Registerin DI classes that will provide Serfice Fabric specific information for logging </summary>
		public static IServiceCollection AddServiceFabricDependencies(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddTransient<IServiceContext, OmexServiceFabricContext>();
			serviceCollection.TryAddSingleton<IMachineInformation, ServiceFabricMachineInformation>();
			return serviceCollection;
		}
	}
}
