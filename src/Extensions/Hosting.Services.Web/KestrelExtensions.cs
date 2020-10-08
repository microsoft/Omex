// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Kestrel extensions
	/// </summary>
	public static class KestrelExtensions
	{
		/// <summary>
		/// Specify Kestrel as the server to be used by the web host.
		/// </summary>
		/// <param name="hostBuilder">Host builder</param>
		/// <param name="configureOptions">Configure options</param>
		/// <param name="serviceProvider">Service provider</param>
		/// <returns>Webhost builder object</returns>
		public static IWebHostBuilder UseKestrel(this IWebHostBuilder hostBuilder, Action<WebHostBuilderContext, IServiceProvider, KestrelServerOptions> configureOptions,
			IServiceProvider serviceProvider)
		{
			return hostBuilder.UseKestrel().ConfigureKestrel(configureOptions, serviceProvider);
		}

		/// <summary>
		/// Configures Kestrel options but does not register an IServer. />.
		/// </summary>
		/// <param name="hostBuilder">
		/// The Microsoft.AspNetCore.Hosting.IWebHostBuilder to configure.
		/// </param>
		/// <param name="configureOptions">A callback to configure Kestrel options.</param>
		/// <param name="serviceProvider">Service provider</param>
		/// <returns>
		/// The Microsoft.AspNetCore.Hosting.IWebHostBuilder.
		/// </returns>
		public static IWebHostBuilder ConfigureKestrel(this IWebHostBuilder hostBuilder,
			Action<WebHostBuilderContext, IServiceProvider, KestrelServerOptions> configureOptions,
			IServiceProvider serviceProvider)
		{
			if (configureOptions == null)
			{
				throw new ArgumentNullException(nameof(configureOptions));
			}

			return hostBuilder.ConfigureServices((context, services) =>
			{
				services.Configure<KestrelServerOptions>(options =>
				{
					configureOptions(context, serviceProvider, options);
				});
			});
		}
	}
}
