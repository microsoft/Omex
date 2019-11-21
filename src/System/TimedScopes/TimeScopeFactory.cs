// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.TimedScopes.ReplayEventLogging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Correlation Data
	/// </summary>
	public class TimedScopeFactory
	{
		/// <summary>
		/// Create a timed scope
		/// </summary>
		/// <param name="customLogger">Use a custom logger for the timed scope</param>
		/// <param name="replayEventConfigurator">Replay event configurator</param>
		/// <param name="timedScopeStackManager">Timed scope stack manager</param>
		/// <param name="correlation">Correlation data</param>
		/// <param name="machineInformation">Machine Information</param>
		public TimedScopeFactory(
			Correlation correlation,
			IMachineInformation machineInformation,
			ITimedScopeLogger customLogger,
			IReplayEventConfigurator replayEventConfigurator,
			ITimedScopeStackManager timedScopeStackManager)
		{
			m_correlation = Code.ExpectsArgument(correlation, nameof(correlation), TaggingUtilities.ReserveTag(0));
			m_machineInformation = Code.ExpectsArgument(machineInformation, nameof(machineInformation), TaggingUtilities.ReserveTag(0));
			m_scopeLogger = Code.ExpectsArgument(customLogger, nameof(customLogger), TaggingUtilities.ReserveTag(0));
			m_replayEventConfigurator = Code.ExpectsArgument(replayEventConfigurator, nameof(replayEventConfigurator), TaggingUtilities.ReserveTag(0));
			m_timedScopeStackManager = Code.ExpectsArgument(timedScopeStackManager, nameof(timedScopeStackManager), TaggingUtilities.ReserveTag(0));
		}


		/// <summary>
		/// Creates a scope (and starts by default)
		/// </summary>
		/// <param name="definition">TimeScope definition</param>
		/// <param name="initialResult">Initial result to use</param>
		/// <param name="startScope">Should the scope be automatically started (for use in e.g. 'using' statement)</param>
		public TimedScope Create(TimedScopeDefinition definition, TimedScopeResult initialResult, bool startScope = true)
		{
			TimedScope scope = definition.Create(m_correlation.CurrentCorrelation, m_machineInformation, m_scopeLogger, m_replayEventConfigurator, m_timedScopeStackManager, initialResult, startScope);
			if (startScope == true)
			{
				scope.Start();
			}

			return scope;
		}


		private readonly Correlation m_correlation;
		private readonly IMachineInformation m_machineInformation;
		private readonly ITimedScopeLogger m_scopeLogger;
		private readonly IReplayEventConfigurator m_replayEventConfigurator;
		private readonly ITimedScopeStackManager m_timedScopeStackManager;
	}
}
