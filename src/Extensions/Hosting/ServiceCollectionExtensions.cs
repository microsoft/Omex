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
		/// AddRule Omex Logging and ActivitySource dependencies
		/// </summary>
		public static IHostBuilder AddOmexServices(this IHostBuilder builder) =>
			builder
				.ConfigureServices((context, collection) => collection.AddOmexServices());

		/// <summary>
		/// AddRule Omex Logging and ActivitySource dependencies
		/// </summary>
		public static IServiceCollection AddOmexServices(this IServiceCollection collection) =>
			collection
				.AddOmexLogging()
				.AddOmexActivitySource();

		/// <summary>
		/// AddRule Omex Logging and ActivitySource dependencies
		/// </summary>
		public static IServiceCollection AddCertificateReader(this IServiceCollection collection)
		{
			collection.TryAddSingleton<ICertificateStore, CertificateStore>();
			collection.TryAddSingleton<ICertificateReader, CertificateReader>();
			return collection;
		}
	}
}
