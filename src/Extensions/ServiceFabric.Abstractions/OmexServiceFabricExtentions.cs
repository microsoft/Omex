// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.ServiceFabric.Abstractions
{
	internal static class OmexServiceFabricExtentions
	{
		public static IServiceCollection AddServiceFabricDependencies(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddSingleton<IMachineInformation, ServiceFabricMachineInformation>();
			return serviceCollection;
		}
	}
}
