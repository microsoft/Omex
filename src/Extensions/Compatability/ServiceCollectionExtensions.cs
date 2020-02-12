// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Omex.Extensions.Compatability
{
	/// <summary>Class to register dependencies for Compatability classes</summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>Initialize depriacated static classes like Code and ULSLogger</summary>
		public static IServiceCollection AddOmexCompatability(this IServiceCollection serviceCollection) =>
			serviceCollection.AddTransient<IHostedService, OmexCompatabilityIntializer>();
	}
}
