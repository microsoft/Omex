// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Activities;
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
		/// Add Omex Logging and TimedScopes dependencies
		/// </summary>
		public static IHostBuilder AddOmexServices(this IHostBuilder builder) =>
			builder
				.ConfigureServices((context, collection) => collection.AddOmexServices());

		/// <summary>
		/// Add Omex Logging and TimedScopes dependencies
		/// </summary>
		public static IServiceCollection AddOmexServices(this IServiceCollection collection) =>
			collection
				.AddOmexLogging()
				.AddOmexActivitySource();

		/// <summary>
		/// Add Omex Logging and TimedScopes dependencies
		/// </summary>
		public static IServiceCollection AddCertificateReader(this IServiceCollection collection) =>
			collection
				.AddSingleton<ICertificateStore, CertificateStore>()
				.AddSingleton<ICertificateReader,CertificateReader>();
	}
}
