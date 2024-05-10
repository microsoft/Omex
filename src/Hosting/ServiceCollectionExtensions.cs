// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
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
		/// </summary>
		[Obsolete($"Please do not use {nameof(AddOmexServices)} if you just want to use Legacy OmexLogger and ActivityEventSender because they are deprecated. Consider using a different telemetry solution. This method is pending for removal by 1 July 2024.", DiagnosticId = "OMEX188")]
		public static IHostBuilder AddOmexServices(this IHostBuilder builder) =>
			builder
				.ConfigureServices((context, collection) => collection.AddOmexServices());

		/// <summary>
		/// Add Omex Logging and ActivitySource dependencies
		/// </summary>
		[Obsolete($"Please do not use {nameof(AddOmexServices)} if you just want to use Legacy OmexLogger and ActivityEventSender because they are deprecated. Consider using a different telemetry solution. This method is pending for removal by 1 July 2024.", DiagnosticId = "OMEX188")]
		public static IServiceCollection AddOmexServices(this IServiceCollection collection) =>
			collection
				.AddOmexLogging()
				.AddOmexActivitySource();

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
