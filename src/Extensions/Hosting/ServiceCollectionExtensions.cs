// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Hosting.Certificates;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.Omex.Extensions.TimedScopes;

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
				.AddTimedScopes()
				.AddExceptionLogging();

		/// <summary>
		/// Add Omex Logging and TimedScopes dependencies
		/// </summary>
		public static IServiceCollection AddCertificateReader(this IServiceCollection collection) =>
			collection
				.AddSingleton<ICertificateStore, CertificateStore>()
				.AddSingleton<ICertificateReader,CertificateReader>();

		/// <summary>
		/// Add logging of exceptions on FirstChanceException
		/// </summary>
		public static IServiceCollection AddExceptionLogging(this IServiceCollection collection)
			=> collection.AddHostedService<ExceptionLoggingHostedService>();

		/// <summary>
		/// Build host and in case of failure report exceptions to event source
		/// </summary>
		/// <param name="builder">Host builder to build</param>
		/// <param name="validateOnBuild">Perform check verifying that all services can be created durin BuildServiceProvider call. NOTE: this check doesn't verify open generics services</param>
		/// <param name="validateScopes">Perform check verifying that scoped services never gets resolved from root provider</param>
		public static IHost BuildWithErrorReporting(this IHostBuilder builder, bool validateOnBuild = true, bool validateScopes = true)
		{
			string name = GetHostName();

			try
			{
				IHost host = builder.UseDefaultServiceProvider(options =>
				{
					options.ValidateOnBuild = validateOnBuild;
					options.ValidateScopes = validateScopes;
				}).Build();

				ServiceInitializationEventSource.Instance.LogHostBuildSucceeded(Process.GetCurrentProcess().Id, name);

				return host;
			}
			catch (Exception exception)
			{
				ServiceInitializationEventSource.Instance.LogHostFailed(exception.ToString(), name);
				throw;
			}
		}

		internal static string GetHostName()
			=> Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownHostName";
	}
}
