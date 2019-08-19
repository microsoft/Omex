// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.TimedScopes.ReplayEventLogging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Store Timed Scope name and its description
	/// </summary>
	public class TimedScopeDefinition
	{
		/// <summary>
		/// Name
		/// </summary>
		public string Name { get; }


		/// <summary>
		/// Description
		/// </summary>
		public string Description { get; }


		/// <summary>
		/// Description
		/// </summary>
		/// <remarks>Could be null</remarks>
		public string LinkToOnCallEngineerHandbook { get; }


		/// <summary>
		/// Should the scope be logged only when explicitly demanded
		/// </summary>
		public bool OnDemand { get; }


		/// <summary>
		/// Does the scope capture user hashes that are suitable for unique user-based alerting?
		/// </summary>
		public bool CapturesUniqueUserHashes { get; }


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="description">Description</param>
		/// <param name="linkToOnCallEngineerHandbook">Link to On Call Engineer Handbook</param>
		/// <param name="onDemand">Should the scope be logged only when explicitly demanded</param>
		/// <param name="capturesUniqueUserHashes">Does the scope capture user hashes that are suitable for unique user-based alerting?</param>
		public TimedScopeDefinition(string name, string description = null, string linkToOnCallEngineerHandbook = null, bool onDemand = false, bool capturesUniqueUserHashes = false)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(name, nameof(name), TaggingUtilities.ReserveTag(0x23817707 /* tag_96x2h */));

			Name = name;
			Description = description ?? string.Empty;
			LinkToOnCallEngineerHandbook = linkToOnCallEngineerHandbook;
			OnDemand = onDemand;
			CapturesUniqueUserHashes = capturesUniqueUserHashes;
		}


		/// <summary>
		/// Starts a scope
		/// </summary>
		/// <param name="correlationData">Correlation Data</param>
		/// <param name="machineInformation">Machine Information</param>
		/// <param name="initialResult">Initial result to use</param>
		/// <param name="customLogger">Optional custom timed scope logger</param>
		/// <param name="replayEventConfigurator">Optional replay event configurator</param>
		/// <param name="timedScopeStackManager">Timed scope stack manager</param>
		/// <returns>A timed scope</returns>
		public TimedScope Start(CorrelationData correlationData, IMachineInformation machineInformation, ITimedScopeLogger customLogger, 
			IReplayEventConfigurator replayEventConfigurator, ITimedScopeStackManager timedScopeStackManager, TimedScopeResult initialResult = default(TimedScopeResult))
			=> Create(correlationData, machineInformation, customLogger, replayEventConfigurator, timedScopeStackManager, initialResult: initialResult, startScope: true);


		/// <summary>
		/// Creates a scope (and starts by default)
		/// </summary>
		/// <param name="correlationData">Correlation Data</param>
		/// <param name="machineInformation">Machine Information</param>
		/// <param name="initialResult">Initial result to use</param>
		/// <param name="startScope">Should the scope be automatically started (for use in e.g. 'using' statement)</param>
		/// <param name="customLogger">Optional custom timed scope logger</param>
		/// <param name="replayEventConfigurator">Optional replay event configurator</param>
		/// <param name="timedScopeStackManager">Timed scope stack manager</param>
		/// <returns>A timed scope</returns>
		public TimedScope Create(CorrelationData correlationData, IMachineInformation machineInformation, ITimedScopeLogger customLogger,  IReplayEventConfigurator replayEventConfigurator, 
			ITimedScopeStackManager timedScopeStackManager, TimedScopeResult initialResult = default(TimedScopeResult), bool startScope = true)
		{
			TimedScope scope = TimedScope.Create(this, correlationData, machineInformation, customLogger, replayEventConfigurator, timedScopeStackManager, initialResult);

			if (startScope)
			{
				scope.Start();
			}

			return scope;
		}


		/// <summary>
		/// Deprecated - Creates a scope
		/// </summary>
		/// <remarks>This overload is obsoleted. Use the overload with TimedScopeResult for new scopes instead.</remarks>
		/// <param name="correlationData">Correlation data</param>
		/// <param name="machineInformation">Machine Information</param>
		/// <param name="initialResult">Initial result to use</param>
		/// <param name="startScope">Should the scope be automatically started (for use in e.g. 'using' statement)</param>
		/// <param name="customLogger">Optional custom timed scope logger</param>
		/// <param name="replayEventConfigurator">Optional replay event configurator</param>
		/// <param name="timedScopeStackManager">Timed scope stack manager</param>
		/// <returns>A timed scope</returns>
		public TimedScope Create(CorrelationData correlationData, IMachineInformation machineInformation, ITimedScopeLogger customLogger, 
			IReplayEventConfigurator replayEventConfigurator, ITimedScopeStackManager timedScopeStackManager, bool? initialResult, bool startScope = true)
			=> Create(correlationData, machineInformation, customLogger, replayEventConfigurator, timedScopeStackManager,
				TimedScope.ConvertBoolResultToTimedScopeResult(initialResult), startScope);
	}
}