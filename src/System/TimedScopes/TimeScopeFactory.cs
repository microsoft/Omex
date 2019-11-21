// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.TimedScopes.ReplayEventLogging;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Correlation Data
	/// </summary>
	public class TimeScopeFactory
	{
		/// <summary>
		/// Correlation Data
		/// </summary>
		public CorrelationData CorrelationData { get; }


		/// <summary>
		/// Timed Scope Definition
		/// </summary>
		public IMachineInformation MachineInformation { get; }


		/// <summary>
		/// Timed Scope metrics logger
		/// </summary>
		private ITimedScopeLogger ScopeLogger { get; }


		/// <summary>
		/// Replay event configurator
		/// </summary>
		private IReplayEventConfigurator ReplayEventConfigurator { get; }


		/// <summary>
		/// Timed Scope Stack Manager
		/// </summary>
		public ITimedScopeStackManager TimedScopeStackManager { get; }


		/// <summary>
		/// Create a timed scope
		/// </summary>
		/// <param name="customLogger">Use a custom logger for the timed scope</param>
		/// <param name="replayEventConfigurator">Replay event configurator</param>
		/// <param name="timedScopeStackManager">Timed scope stack manager</param>
		/// <param name="correlationData">Correlation data</param>
		/// <param name="machineInformation">Machine Information</param>
		public TimeScopeFactory(
			CorrelationData correlationData,
			IMachineInformation machineInformation,
			ITimedScopeLogger customLogger,
			IReplayEventConfigurator replayEventConfigurator,
			ITimedScopeStackManager timedScopeStackManager)
		{
			CorrelationData = correlationData;
			MachineInformation = machineInformation;
			ScopeLogger = customLogger;
			ReplayEventConfigurator = replayEventConfigurator;
			TimedScopeStackManager = timedScopeStackManager;
		}


		/// <summary>
		/// Creates a scope (and starts by default)
		/// </summary>
		/// <param name="definition">TimeScope definition</param>
		/// <param name="initialResult">Initial result to use</param>
		/// <param name="startScope">Should the scope be automatically started (for use in e.g. 'using' statement)</param>
		public TimedScope Create(TimedScopeDefinition definition, TimedScopeResult initialResult, bool startScope = true)
		{
			TimedScope scope = definition.Create(CorrelationData, MachineInformation, ScopeLogger, ReplayEventConfigurator, TimedScopeStackManager, initialResult, startScope);
			if (startScope == true)
			{
				scope.Start();
			}

			return scope;
		}
	}
}
