// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.TimedScopes;
using Microsoft.Omex.System.TimedScopes.ReplayEventLogging;

namespace Microsoft.Omex.System.AspNetCore
{
	/// <summary>
	/// Extension methods for the TimedScopes
	/// </summary>
	public static class TimedScopesExtensions
	{
		/// <summary>
		/// Configures TimedScopes
		/// </summary>
		/// <param name="app">Application builder</param>
		/// <returns>Application builder</returns>
		public static IWebHostBuilder UseTimedScopes(this IWebHostBuilder app)
		{
			app.ConfigureServices(
				service => service
					.AddSingleton<IMachineInformation, BasicMachineInformation>()
					.AddSingleton<ICorrelationStorage, MemoryCorrelationHandler>()
					.AddSingleton<ICorrelationDataProvider, Correlation>()
					//.AddSingleton<ITimedScopeLogger, TimedScopeLogger>()
					.AddSingleton<IReplayEventConfigurator, ReplayEventConfigurator>()
					.AddSingleton<ICallContextManager, CallContextManager>()
					.AddSingleton<ITimedScopeStackManager, TimedScopeStackManager>()
					.AddSingleton<ITimedScopeProvider, TimedScopeProvider>()
			);

			return app;
		}
	}
}
