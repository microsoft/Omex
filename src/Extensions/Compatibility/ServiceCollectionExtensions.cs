// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Omex.Extensions.Compatibility
{
	/// <summary>
	/// Class to register dependencies for Compatibility classes
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Initialize deprecated static classes like Code and ULSLogger
		/// </summary>
		public static IHostBuilder AddOmexCompatibilityServices(this IHostBuilder builder) =>
			builder.ConfigureServices((context, collection) =>
			{
				collection.AddHostedService<OmexCompatibilityIntializerService>();
			});
	}
}
