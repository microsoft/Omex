// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.TimedScopes;
using Microsoft.Omex.System.TimedScopes.ReplayEventLogging;

namespace Microsoft.Omex.System.UnitTests.Shared.TimedScopes
{
	/// <summary>
	/// Bridge for tests accessing internal classes.
	/// </summary>
	public static class TestHooks
	{
		/// <summary>
		/// Default scope name.
		/// </summary>
		public static string DefaultTimedScopeName = TimedScopes.DefaultScope.Name;


		/// <summary>
		/// Creates a default timed scope.
		/// </summary>
		/// <param name="scopeLogger">Custom logger</param>
		/// <param name="replayEventConfigurator">Reply event configurator</param>
		/// <param name="machineInformation">Machine information</param>
		/// <param name="timedScopeStackManager">Timed scope stack manager</param>
		/// <param name="initialResult">Initial scope result.</param>
		/// <param name="startScope">Start scope implicitly.</param>
		/// <returns>The created scope.</returns>
		public static TimedScope CreateDefaultTimedScope(
			ITimedScopeLogger scopeLogger = null,
			IReplayEventConfigurator replayEventConfigurator = null,
			IMachineInformation machineInformation = null,
			ITimedScopeStackManager timedScopeStackManager = null,
			bool? initialResult = null,
			bool startScope = true)
		{
			CorrelationData data = new CorrelationData();

			return UnitTestTimedScopes.DefaultScope.Create(data, machineInformation, scopeLogger, replayEventConfigurator, timedScopeStackManager, initialResult, startScope);
		}


		/// <summary>
		/// Creates an instance of the test counters timed scope.
		/// </summary>
		/// <param name="initialResult">Initial scope result.</param>
		/// <param name="startScope">Start scope implicitly</param>
		/// <param name="machineInformation">Machine information</param>
		/// <param name="scopeLogger">Custom logger</param>
		/// <param name="replayEventConfigurator">Replay event configurator</param>
		/// <param name="timedScopeStackManager">Timed scope stack manager</param>
		/// <returns>The created scope.</returns>
		public static TimedScope CreateTestCountersUnitTestTimedScope(
			bool? initialResult = null,
			bool startScope = true,
			IMachineInformation machineInformation = null,
			ITimedScopeLogger scopeLogger = null, 
			IReplayEventConfigurator replayEventConfigurator = null,
			ITimedScopeStackManager timedScopeStackManager = null)
		{

			CorrelationData data = new CorrelationData();

			return UnitTestTimedScopes.TestCounters.UnitTest.Create(data, machineInformation, scopeLogger, replayEventConfigurator, timedScopeStackManager, initialResult, startScope);
		}
	}
}
