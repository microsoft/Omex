// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
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
			return CreateTimedScopeProvider(machineInformation, scopeLogger, replayEventConfigurator, timedScopeStackManager)
				.Create(UnitTestTimedScopes.DefaultScope, TimedScope.ConvertBoolResultToTimedScopeResult(initialResult), startScope);
		}

		/// <summary>
		/// Creates a default timed scope provider.
		/// </summary>
		/// <param name="scopeLogger">Custom logger</param>
		/// <param name="replayEventConfigurator">Reply event configurator</param>
		/// <param name="machineInformation">Machine information</param>
		/// <param name="timedScopeStackManager">Timed scope stack manager</param>
		public static ITimedScopeProvider CreateTimedScopeProvider(
			IMachineInformation machineInformation,
			ITimedScopeLogger scopeLogger,
			IReplayEventConfigurator replayEventConfigurator,
			ITimedScopeStackManager timedScopeStackManager)
		{
			return new TimedScopeProvider(new MockCorrelationDataProvider(), machineInformation, scopeLogger, replayEventConfigurator, timedScopeStackManager);
		}

		private class MockCorrelationDataProvider : ICorrelationDataProvider
		{
			public CorrelationData CurrentCorrelation => new CorrelationData();
		}
	}
}
