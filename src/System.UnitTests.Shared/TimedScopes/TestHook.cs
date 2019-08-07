// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.System.TimedScopes;
using Microsoft.Omex.System.TimedScopes.ReplayEventLogging;
using Microsoft.Omex.System.UnitTests.Shared.Diagnostics;

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
		/// <param name="scopeLogger"></param>
		/// <param name="replayEventConfigurator"></param>
		/// <param name="initialResult">Initial scope result.</param>
		/// <param name="startScope">Start scope implicitly.</param>
		/// <returns>The created scope.</returns>
		public static TimedScope CreateDefaultTimedScope(
			ITimedScopeLogger scopeLogger = null,
			IReplayEventConfigurator replayEventConfigurator = null,
			bool? initialResult = null,
			bool startScope = true)
		{
			CorrelationData data = new CorrelationData();

			return UnitTestTimedScopes.DefaultScope.Create(data, new UnitTestMachineInformation(), initialResult, startScope, scopeLogger, replayEventConfigurator);
		}


		/// <summary>
		/// Creates an instance of the test counters timed scope.
		/// </summary>
		/// <param name="initialResult">Initial scope result.</param>
		/// <param name="startScope">Start scope implicitly.</param>
		/// <param name="scopeLogger"></param>
		/// <param name="replayEventConfigurator"></param>
		/// <returns>The created scope.</returns>
		public static TimedScope CreateTestCountersUnitTestTimedScope(
			bool? initialResult = null,
			bool startScope = true, 
			ITimedScopeLogger scopeLogger = null, 
			IReplayEventConfigurator replayEventConfigurator = null)
		{

			CorrelationData data = new CorrelationData();

			return UnitTestTimedScopes.TestCounters.UnitTest.Create(data, new UnitTestMachineInformation(), initialResult, startScope, scopeLogger, replayEventConfigurator);
		}
	}
}
