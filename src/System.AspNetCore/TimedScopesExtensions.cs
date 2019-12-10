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
		/// Register types required for timedscopes
		/// </summary>
		public static IServiceCollection AddTimedScopes(this IServiceCollection serviceCollection)
		{
			return serviceCollection
			    .AddSingleton<ICallContextManager, CallContextManager>()
				.AddSingleton<ICorrelationDataProvider, Correlation>()
				.AddSingleton<ICorrelationStorage, MemoryCorrelationHandler>()
				.AddSingleton<IMachineInformation, BasicMachineInformation>()
				.AddSingleton<ITimedScopeLogger, CustomActivityTimedScopeLogger>()
				.AddSingleton<ITimedScopeProvider, TimedScopeProvider>()
				.AddSingleton<ITimedScopeStackManager, TimedScopeStackManager>()
				.AddSingleton<IReplayEventConfigurator, ReplayEventConfigurator>()
				.AddSingleton<TimedScopeEventSource>();
		}
	}
}
