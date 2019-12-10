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
					.AddSingleton<IMachineInformation, BasicMachineInformation>()
					.AddSingleton<ICorrelationStorage, MemoryCorrelationHandler>()
					.AddSingleton<ICorrelationDataProvider, Correlation>()
					.AddSingleton<TimedScopeEventSource>()
					.AddSingleton<ITimedScopeLogger, CustomActivityTimedScopeLogger>()
					.AddSingleton<IReplayEventConfigurator, ReplayEventConfigurator>()
					.AddSingleton<ICallContextManager, CallContextManager>()
					.AddSingleton<ITimedScopeStackManager, TimedScopeStackManager>()
					.AddSingleton<ITimedScopeProvider, TimedScopeProvider>();
		}
	}
}
