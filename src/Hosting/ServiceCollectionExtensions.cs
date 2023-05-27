// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Hosting.Certificates;
using Microsoft.Omex.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Hosting
{
	/// <summary>
	/// Extension to add Omex dependencies to HostBuilder
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Add Omex Logging and ActivitySource dependencies
		/// <param name="builder">Host builder</param>
		/// <param name="disableOmexLogging">Disable OmexLogger if you want to use your custom logger, eg: OpenTelemetry</param>
		/// </summary>
		/// <returns>Host Builder</returns>
		public static IHostBuilder AddOmexServices(this IHostBuilder builder, bool disableOmexLogging = false) =>
			builder
				.ConfigureServices((context, collection) => collection.AddOmexServices(disableOmexLogging));

		/// <summary>
		/// Add Omex Logging and ActivitySource dependencies
		/// </summary>
		/// <param name="collection">Service Collection for DI</param>
		/// <param name="disableOmexLogging">Disable OmexLogger if you want to use your custom logger, eg: OpenTelemetry</param>
		/// <returns>Service Collection</returns>
		public static IServiceCollection AddOmexServices(this IServiceCollection collection, bool disableOmexLogging = false)
		{
			collection.AddOmexActivitySource();

			if (!disableOmexLogging)
			{
				collection.AddOmexLogging();
			}

			return collection;
		}

		/// <summary>
		/// Add Omex Logging and ActivitySource dependencies
		/// </summary>
		public static IServiceCollection AddCertificateReader(this IServiceCollection collection)
		{
			collection.TryAddSingleton<ICertificateStore, CertificateStore>();
			collection.TryAddSingleton<ICertificateReader, CertificateReader>();
			return collection;
		}
	}
}
