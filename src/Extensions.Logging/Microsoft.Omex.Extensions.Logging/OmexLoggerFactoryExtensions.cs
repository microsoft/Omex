// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Logging.Diagnostics;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Extension methods for the <see cref="ILoggerFactory"/> class.
	/// </summary>
	public static class OmexLoggerFactoryExtensions
	{
		/// <summary>
		/// Adds an event logger named 'EventLog' to the factory.
		/// </summary>
		/// <param name="builder">The extension method argument.</param>
		/// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
		public static ILoggingBuilder AddEventLog<TServiceContext>(this ILoggingBuilder builder)
			where TServiceContext : class, IServiceContext
		{
			builder.Services.TryAddSingleton<IServiceContext, TServiceContext>();
			builder.Services.TryAddSingleton<IMachineInformation, BasicMachineInformation>();
			builder.Services.TryAddSingleton<IExternalScopeProvider, LoggerExternalScopeProvider>();
			builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, OmexLoggerProvider>());
			return builder;
		}
	}
}
