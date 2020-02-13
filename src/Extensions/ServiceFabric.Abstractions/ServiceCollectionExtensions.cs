// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddServiceFabricDependencies(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddTransient<IServiceContext, OmexServiceFabricContext>();
			serviceCollection.TryAddTransient<IMachineInformation, ServiceFabricMachineInformation>();
			return serviceCollection;
		}
	}
}
