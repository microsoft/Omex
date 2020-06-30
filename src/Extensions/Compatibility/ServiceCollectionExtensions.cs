// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Compatibility;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Class to register dependencies for Compatibility classes
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Initialize deprecated static classes like Code and ULSLogger
		/// </summary>
		public static IServiceCollection AddOmexCompatibilityServices(this IServiceCollection services) =>
			services.AddHostedService<OmexCompatibilityIntializerService>();
	}
}
