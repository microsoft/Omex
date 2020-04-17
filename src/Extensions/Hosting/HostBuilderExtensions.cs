// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.Omex.Extensions.TimedScopes;

namespace Microsoft.Omex.Extensions.Hosting
{
	/// <summary>
	/// Extension to add Omex dependencies to HostBuilder
	/// </summary>
	public static class HostBuilderExtensions
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
				.AddTimedScopes();

		/// <summary>
		/// Build generic host and in case of initialization error propagate exception to log aggregator
		/// </summary>
		/// <remarks>
		/// Method will also try to register Omex types if they are not present
		/// </remarks>
		/// <param name="builder">Host build to build</param>
		/// <param name="applicationName">Name of the application that running in generic host</param>
		/// <param name="additionalActions">actions that could also produce exception that should be reported</param>
		public static IHost BuildWithReporting(this IHostBuilder builder, string applicationName, Action<IHostBuilder>? additionalActions = null)
		{
			string applicationNameForLogging = applicationName;

			try
			{
				if (string.IsNullOrWhiteSpace(applicationName))
				{
					applicationNameForLogging = Assembly.GetExecutingAssembly().GetName().FullName;
					throw new ArgumentException("Should not be null or empty", nameof(applicationName));
				}

				// for generic host application name is the name of the service that it's running (don't confuse with Sf application name)
				builder.UseApplicationName(applicationName);

				additionalActions?.Invoke(builder);

				IHost host = builder
					.AddOmexServices()
					.UseDefaultServiceProvider(options =>
					{
						options.ValidateOnBuild = true;
						options.ValidateScopes = true;
					})
					.Build();

				ServiceInitializationEventSource.Instance.LogHostBuildSucceeded(Process.GetCurrentProcess().Id, applicationNameForLogging);

				return host;
			}
			catch (Exception e)
			{
				ServiceInitializationEventSource.Instance.LogHostBuildFailed(e.ToString(), applicationNameForLogging);
				throw;
			}
		}

		/// <summary>
		/// Overides ApplicationName in host configuration
		/// </summary>
		/// <remarks>
		/// Method done internal instead of private to create unit tests for it,
		/// since failure to set proper application name could cause Service Fabric error that is hard to debug:
		///    System.Fabric.FabricException: Invalid Service Type
		/// </remarks>
		internal static IHostBuilder UseApplicationName(this IHostBuilder builder, string applicationName) =>
			builder.ConfigureHostConfiguration(configuration =>
			{
				configuration.AddInMemoryCollection(new[]
				{
					new KeyValuePair<string, string>(
						HostDefaults.ApplicationKey,
						string.IsNullOrWhiteSpace(applicationName)
							? throw new ArgumentNullException(nameof(applicationName))
							: applicationName)
				});
			});
	}
}
