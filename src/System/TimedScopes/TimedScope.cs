// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading;
using Microsoft.Omex.System.Context;
using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;
using Microsoft.Omex.System.TimedScopes.ReplayEventLogging;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Class for managing timed scopes, writing to performance
	/// monitor and loggign to ULS if test transactions are running
	/// </summary>
	public class TimedScope : IDisposable
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="scopeDefinition">Timed scope definition</param>
		/// <param name="scopeLogger">Scope metrics logger</param>
		/// <param name="replayEventConfigurator">Replay event configurator</param>
		/// <param name="machineInformation">Machine Information</param>
		/// <param name="correlationData">Correlation data</param>
		private TimedScope(TimedScopeDefinition scopeDefinition, CorrelationData correlationData, ITimedScopeLogger scopeLogger, 
			IReplayEventConfigurator replayEventConfigurator, IMachineInformation machineInformation)
		{
			Code.ExpectsArgument(scopeDefinition, nameof(scopeDefinition), TaggingUtilities.ReserveTag(0));
			Code.ExpectsArgument(scopeLogger, nameof(scopeLogger), TaggingUtilities.ReserveTag(0));
			Code.ExpectsArgument(replayEventConfigurator, nameof(replayEventConfigurator), TaggingUtilities.ReserveTag(0));

			ScopeDefinition = scopeDefinition;
			ScopeLogger = scopeLogger;
			ReplayEventConfigurator = replayEventConfigurator;
			CorrelationData = correlationData;
			MachineInformation = machineInformation;
		}


		/// <summary>
		/// Create a timed scope
		/// </summary>
		/// <param name="scopeDefinition">Timed scope definition</param>
		/// <param name="initialResult">The default result for the scope</param>
		/// <param name="customLogger">Use a custom logger for the timed scope</param>
		/// <param name="replayEventConfigurator">Replay event configurator</param>
		/// <param name="correlationData">Correlation data</param>
		/// <param name="machineInformation">Machine Information</param>
		/// <returns>Newly created scope</returns>
		public static TimedScope Create(TimedScopeDefinition scopeDefinition, CorrelationData correlationData, IMachineInformation machineInformation,
			TimedScopeResult initialResult = default(TimedScopeResult), ITimedScopeLogger customLogger = null, IReplayEventConfigurator replayEventConfigurator = null)
		{
			return new TimedScope(scopeDefinition, correlationData, customLogger, replayEventConfigurator, machineInformation)
			{
				TimedScopeData = correlationData,
				RunningTransaction = TransactionMonitor.RunningTransaction(correlationData),
				Result = initialResult,
			};
		}


		/// <summary>
		/// Deprecated - Start a timed scope
		/// </summary>
		/// <remarks>Please use TimedScopeDefinition for creating timed scopes</remarks>
		/// <param name="correlationData">Correlation data</param>
		/// <param name="machineInformation">Machine Information</param>
		/// <param name="scopeName">The name of the timed scope</param>
		/// <param name="initialResult">The default result for the scope</param>
		/// <param name="customLogger">Use a custom logger for the timed scope</param>
		/// <param name="replayEventConfigurator">Replay event configurator</param>
		/// <returns>Newly created scope</returns>
		public static TimedScope Start(CorrelationData correlationData, IMachineInformation machineInformation, string scopeName, TimedScopeResult initialResult = default(TimedScopeResult),
			ITimedScopeLogger customLogger = null, IReplayEventConfigurator replayEventConfigurator = null)
				=> new TimedScopeDefinition(scopeName).Start(correlationData, machineInformation, initialResult, customLogger, replayEventConfigurator);


		/// <summary>
		/// Deprecated - Start a timed scope
		/// </summary>
		/// <remarks>Please use TimedScopeDefinition for creating timed scopes</remarks>
		/// <param name="correlationData">Correlation data</param>
		/// <param name="machineInformation">Machine Information</param>
		/// <param name="scopeName">The name of the timed scope</param>
		/// <param name="initialResult">The default result for the scope</param>
		/// <returns>Newly created scope</returns>
		public static TimedScope Start(CorrelationData correlationData, IMachineInformation machineInformation, string scopeName, bool? initialResult)
			=> new TimedScopeDefinition(scopeName).Start(correlationData, machineInformation, ConvertBoolResultToTimedScopeResult(initialResult));


		/// <summary>
		/// Deprecated - Start a timed scope
		/// </summary>
		/// <remarks>Please use TimedScopeDefinition for creating timed scopes</remarks>
		/// <param name="correlationData">Correlation Data</param>
		/// <param name="machineInformation">Machine Information</param>
		/// <param name="scopeName">The name of the timed scope</param>
		/// <param name="description">The description of the timed scope</param>
		/// <param name="initialResult">The default result for the scope</param>
		/// <param name="customLogger">Use a custom logger for the timed scope</param>
		/// <param name="replayEventConfigurator">Replay event configurator</param>
		/// <returns>Newly created scope</returns>
		public static TimedScope Start(CorrelationData correlationData, IMachineInformation machineInformation, string scopeName, string description,
			TimedScopeResult initialResult = default(TimedScopeResult), ITimedScopeLogger customLogger = null, IReplayEventConfigurator replayEventConfigurator = null)
				=> new TimedScopeDefinition(scopeName, description).Start(correlationData, machineInformation, initialResult, customLogger, replayEventConfigurator);


		/// <summary>
		/// Deprecated - Start a timed scope
		/// </summary>
		/// <remarks>Please use TimedScopeDefinition for creating timed scopes</remarks>
		/// <param name="correlationData">Correlation Data</param>
		/// <param name="machineInformation">Machine Information</param>
		/// <param name="scopeName">The name of the timed scope</param>
		/// <param name="description">The description of the timed scope</param>
		/// <param name="initialResult">The default result for the scope</param>
		/// <returns>Newly created scope</returns>
		public static TimedScope Start(CorrelationData correlationData, IMachineInformation machineInformation, string scopeName, string description, bool? initialResult)
			=> new TimedScopeDefinition(scopeName, description).Start(correlationData, machineInformation, ConvertBoolResultToTimedScopeResult(initialResult));


		/// <summary>
		/// Deprecated - Create a timed scope
		/// </summary>
		/// <remarks>Please use TimedScopeDefinition for creating timed scopes</remarks>
		/// <param name="correlationData">Correlation data</param>
		/// <param name="machineInformation">Machine Information</param>
		/// <param name="scopeName">The name of the timed scope</param>
		/// <param name="description">The description of the timed scope</param>
		/// <param name="initialResult">The default result for the scope</param>
		/// <param name="customLogger">Use a custom logger for the timed scope</param>
		/// <param name="replayEventConfigurator">Replay event configurator</param>
		/// <returns>newly created scope</returns>
		public static TimedScope Create(CorrelationData correlationData, IMachineInformation machineInformation, string scopeName, string description,
			TimedScopeResult initialResult = default(TimedScopeResult), ITimedScopeLogger customLogger = null, IReplayEventConfigurator replayEventConfigurator = null)
			=> new TimedScopeDefinition(scopeName, description).Create(correlationData, machineInformation, initialResult, startScope: false, customLogger: customLogger,
				replayEventConfigurator: replayEventConfigurator);


		/// <summary>
		/// Deprecated - Create a timed scope
		/// </summary>
		/// <remarks>Please use TimedScopeDefinition for creating timed scopes</remarks>
		/// <param name="correlationData">Correlation data</param>
		/// <param name="machineInformation">Machine Information</param>
		/// <param name="scopeName">The name of the timed scope</param>
		/// <param name="description">The description of the timed scope</param>
		/// <param name="initialResult">The default result for the scope</param>
		/// <returns>Newly created scope</returns>
		public static TimedScope Create(CorrelationData correlationData, IMachineInformation machineInformation, string scopeName, string description, bool? initialResult)
			=> new TimedScopeDefinition(scopeName, description).Create(correlationData, machineInformation, ConvertBoolResultToTimedScopeResult(initialResult), startScope: false);


		/// <summary>
		/// Gets the current active timed scope, or null if there are none active
		/// </summary>
		public static TimedScope Current => Scopes?.Peek();


		/// <summary>
		/// Parent Scope if available
		/// </summary>
		public TimedScope Parent { get; private set; }


		/// <summary>
		/// Start the timed scope
		/// </summary>
		public void Start()
		{
			if (IsDisposed)
			{
				ULSLogging.LogTraceTag(0x23850298 /* tag_97qky */, Categories.TimingGeneral, Levels.Error,
					"Attempting to start scope '{0}' that has already been disposed.", Name);
				return;
			}

			if (IsScopeActive)
			{
				ULSLogging.LogTraceTag(0x23850299 /* tag_97qkz */, Categories.TimingGeneral, Levels.Error,
					"Attempting to start scope '{0}' that has already been started.", Name);
				return;
			}

			string metaDataCopy = MetaData;
			string subTypeCopy = SubType;

			CorrelationData currentCorrelation = CorrelationData;
			TimedScopeData = currentCorrelation.Clone() ?? new CorrelationData();
			RunningTransaction = TransactionMonitor.RunningTransaction(TimedScopeData);

			if (!string.IsNullOrWhiteSpace(metaDataCopy) && string.IsNullOrWhiteSpace(MetaData))
			{
				MetaData = metaDataCopy;
			}

			if (!string.IsNullOrWhiteSpace(subTypeCopy) && string.IsNullOrWhiteSpace(SubType))
			{
				SubType = subTypeCopy;
			}

			// differentiate scope name when running under a test transaction
			if (IsTransaction)
			{
				NameSuffix = string.Concat(NameSuffix, "::Trx", RunningTransaction.ToString(CultureInfo.InvariantCulture));
			}

			// differentiate special scopes
			if (TimedScopeData.IsFallbackCall)
			{
				NameSuffix = string.Concat(NameSuffix, "::Fallback");
			}

			// differentiate scope name for inner (proxied) calls
			if (TimedScopeData.CallDepth > 0)
			{
				NameSuffix = string.Concat(NameSuffix, "::Depth", TimedScopeData.CallDepth.ToString(CultureInfo.InvariantCulture));

				if (currentCorrelation != null)
				{
					// reset call depth so any inner scopes are reported as layer 0 again
					currentCorrelation.CallDepth = 0;
				}
			}

			Parent = TimedScope.Current;
			IsRoot = Parent == null && TimedScopeData.CallDepth == 0;
			StartTick = Stopwatch.GetTimestamp();
			IsScopeActive = true;
			ScopeLogger.LogScopeStart(this);

			PerfDiagnostics = new PerfDiagnostics(Parent != null ? Parent.PerfDiagnostics : null);
			PerfDiagnostics.Start();
		}


		/// <summary>
		/// End the timed scope
		/// </summary>
		public void End(IMachineInformation machineInformation)
		{
			if (IsDisposed)
			{
				ULSLogging.LogTraceTag(0x2385029a /* tag_97qk0 */, Categories.TimingGeneral, Levels.Error,
					"Attempting to end scope '{0}' that has already been disposed.", Name);
				return;
			}

			EndScope(machineInformation);
		}


		/// <summary>
		/// Discard the timed scope, aborting timer and avoiding any data being logged
		/// </summary>
		public void Discard()
		{
			if (Interlocked.CompareExchange(ref m_isScopeEnded, 1, 0) == 0)
			{
				IsScopeActive = false;
			}
		}


		/// <summary>
		/// Timed Scope Definition
		/// </summary>
		public TimedScopeDefinition ScopeDefinition { get; private set; }

		/// <summary>
		/// Timed Scope Definition
		/// </summary>
		public IMachineInformation MachineInformation { get; private set; }


		/// <summary>
		/// The name of the timed scope
		/// </summary>
		public string Name => string.Concat(ScopeDefinition.Name, NameSuffix);


		/// <summary>
		/// Timed Scope Name suffix
		/// </summary>
		private string NameSuffix { get; set; } = string.Empty;


		/// <summary>
		/// Subtype
		/// </summary>
		public string SubType
		{
			set { AddLoggingValue(TimedScopeDataKeys.SubType, value, overrideValue: true); }
			get { return TimedScopeData.Data(TimedScopeDataKeys.SubType); }
		}


		/// <summary>
		/// Metadata
		/// </summary>
		public string MetaData
		{
			set { AddLoggingValue(TimedScopeDataKeys.ObjectMetaData, value, overrideValue: true); }
			get { return TimedScopeData.Data(TimedScopeDataKeys.ObjectMetaData); }
		}


		/// <summary>
		/// Unique Id for this timed scope
		/// </summary>
		public Guid InstanceId { get; } = Guid.NewGuid();


		/// <summary>
		/// Description of the timed scope
		/// </summary>
		public string Description => ScopeDefinition.Description;


		/// <summary>
		/// Is this the outermost scope in a call stack
		/// </summary>
		public bool IsRoot { get; private set; }


		/// <summary>
		/// The Performance diagnostics instance
		/// </summary>
		public PerfDiagnostics PerfDiagnostics { get; private set; }


		/// <summary>
		/// Timed Scope metrics logger
		/// </summary>
		private ITimedScopeLogger ScopeLogger { get; }


		/// <summary>
		/// Replay event configurator
		/// </summary>
		private IReplayEventConfigurator ReplayEventConfigurator { get; }


		public CorrelationData CorrelationData { get; private set; }


		/// <summary>
		/// Id of the counterset
		/// </summary>
		public uint Id
		{
			get
			{
				unchecked
				{
					return (uint)Name.GetHashCode();
				}
			}
		}


		/// <summary>
		/// Explicit duration property
		/// </summary>
		public TimeSpan? Duration { get; set; }


		/// <summary>
		/// Starting tick of the timed scope
		/// </summary>
		public long StartTick { get; private set; }


		/// <summary>
		/// Ending tick of the timed scope
		/// </summary>
		public long EndTick { get; private set; }


		/// <summary>
		/// Scope duration in milliseconds
		/// </summary>
		public double DurationInMilliseconds
		{
			get
			{
				if (Duration.HasValue)
				{
					return Duration.Value.TotalMilliseconds;
				}
				else if (EndTick != 0)
				{
					return Math.Round((EndTick - StartTick) * 1000d / Frequency, 2);
				}
				else
				{
					return Math.Round((Stopwatch.GetTimestamp() - StartTick) * 1000d / Frequency, 2);
				}
			}
		}


		/// <summary>
		/// The frequency for the timed scope
		/// </summary>
		public static long Frequency
		{
			get
			{
				return Stopwatch.Frequency;
			}
		}


		/// <summary>
		/// Failure description
		/// </summary>
		/// <remarks>
		/// This field is only exposed in ULS logs and the MDS timed scope stream.
		/// </remarks>
		public Enum FailureDescription { get; set; }


		/// <summary>
		/// Result of the timed scope.
		/// </summary>
		/// <remarks>
		/// You should set this property instead of IsSuccessful property. You shouldn't set both of them as one rewrites the other.
		/// Setting this property is strongly prefered over authoring new heuristic rules.
		/// </remarks>
		public TimedScopeResult Result { get; set; }


		/// <summary>
		/// Is the timed scope successful
		/// </summary>
		/// <remarks>
		/// Setting this property is obsoleted and shouldn't be used in new code. You should set Result property directly. This property internally
		/// sets the Result property anyway. We do not remove the property setter because of the backward compatibility.
		/// </remarks>
		public bool? IsSuccessful
		{
			get
			{
				return Result.IsSuccessful();
			}

			set
			{
				Result = ConvertBoolResultToTimedScopeResult(value);
			}
		}


		/// <summary>
		/// Is running transaction
		/// </summary>
		public bool IsTransaction => RunningTransaction != Transactions.None;


		/// <summary>
		/// Scope ended flag - 0 not ended, 1 ended
		/// </summary>
		private int m_isScopeEnded;


		/// <summary>
		/// Is the timed scope active
		/// </summary>
		private bool m_isScopeActive;


		/// <summary>
		/// UserHash override
		/// </summary>
		private string m_userHashOverride = null;


		/// <summary>
		/// Sets the user hash override
		/// </summary>
		/// <param name="userHash">User hash</param>
		public void OverrideUserHash(string userHash)
		{
			if (!Code.ValidateNotNullOrWhiteSpaceArgument(userHash, nameof(userHash), TaggingUtilities.ReserveTag(0)))
			{
				return;
			}

			m_userHashOverride = userHash;
		}


		/// <summary>
		/// Is the timed scope active
		/// </summary>
		public bool IsScopeActive
		{
			get
			{
				return m_isScopeActive;
			}
			private set
			{
				if (value != m_isScopeActive)
				{
					ModifyActiveScopes(value);
				}

				m_isScopeActive = value;
			}
		}


		/// <summary>
		/// Gets or sets a flag indicating if Verbose ULS capture should be disabled if this scope fails
		/// </summary>
		public bool DisableVerboseUlsCapture { get; set; }


		/// <summary>
		/// Abort the timer
		/// </summary>
		/// <param name="success">true if action should be considered successful</param>
		public void AbortTimer(bool? success = null)
		{
			IsScopeActive = false;
			if (success.HasValue)
			{
				IsSuccessful = success;
			}
		}


		/// <summary>
		/// Adds the given key and value to the context of the timed scope
		/// </summary>
		/// <param name="key">Key of the item to be added.</param>
		/// <param name="value">Value of the item to be added.</param>
		/// <param name="overrideValue">Whether the value should be overriden</param>
		public void AddLoggingValue(string key, string value, bool overrideValue = false)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				ULSLogging.LogTraceTag(0x2385029b /* tag_97qk1 */, Categories.TimingGeneral, Levels.Error,
					"Empty or null key detected when attempting to add an entry in the timed scope data dictionary. Key : '{0}'.",
					key ?? "<NULL>");
				return;
			}

			string existingValue = TimedScopeData.Data(key);

			if (existingValue != null && !overrideValue)
			{
				ULSLogging.LogTraceTag(0x2385029c /* tag_97qk2 */, Categories.TimingGeneral, Levels.Warning,
					"Timed scope data dictionary already contains key '{0}' with value '{1}'.", key, existingValue);
			}
			else
			{
				TimedScopeData.AddData(key, value ?? string.Empty);
			}
		}


		/// <summary>
		/// Set success value from a passed in status code
		/// </summary>
		/// <param name="statusCode">status code to set success value from</param>
		/// <remarks>500 or above is considered failure,
		/// all other values considered success. However, if the success value is already set,
		/// calling this method does not override the success value.
		/// The status code is stored on the scope in any case.</remarks>
		public void SetSuccessFromStatusCode(HttpStatusCode statusCode)
		{
			if (!IsSuccessful.HasValue)
			{
				IsSuccessful = (uint)statusCode < (uint)HttpStatusCode.InternalServerError;
			}

			if (string.IsNullOrWhiteSpace(TimedScopeData.Data(TimedScopeDataKeys.InternalOnly.StatusCode)))
			{
				AddLoggingValue(TimedScopeDataKeys.InternalOnly.StatusCode, statusCode.ToString());
			}
		}


		/// <summary>
		/// Is the timed scope disposed
		/// </summary>
		private bool IsDisposed { get; set; }


		/// <summary>
		/// End the timed scope explicitly
		/// </summary>
		private void EndScope(IMachineInformation machineInformation)
		{
			if (Interlocked.CompareExchange(ref m_isScopeEnded, 1, 0) == 0)
			{
				EndTick = Stopwatch.GetTimestamp();

				PerfDiagnostics?.Stop();

				LogEnd(machineInformation);

				IsScopeActive = false;
			}
		}


		/// <summary>
		/// Logs the scope end to ULS
		/// </summary>
		private void LogEnd(IMachineInformation machineInformation)
		{
			if (!IsSuccessful.HasValue)
			{
				ULSLogging.LogTraceTag(0x2385029d /* tag_97qk3 */, Categories.TimingGeneral, Levels.Warning,
					"Result not set for scope {0}. Considered as SystemError", Name);

				Result = TimedScopeResult.SystemError;
				FailureDescription = InternalFailureDescription.UnknownResultAsSystemError;
			}

			CorrelationData scopeData = ConstructCorrelationDataEntries(machineInformation);
			ScopeLogger.LogScopeEnd(this, scopeData);

			ReplayEventConfigurator.ConfigureReplayEventsOnScopeEnd(this);
		}

		/// <summary>
		/// Dispose of the timed scope
		/// </summary>
		public void Dispose()
		{
			Dispose(true, MachineInformation);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Dispose of the timed scope
		/// </summary>
		/// <param name="disposing">Should dispose or not</param>
		/// <param name="machineInformation">Machine Information</param>
		protected virtual void Dispose(bool disposing, IMachineInformation machineInformation)
		{
			if (disposing && !IsDisposed)
			{
				IsDisposed = true;
				EndScope(machineInformation);
			}
		}


		/// <summary>
		/// Timed scope data, added to the log output
		/// </summary>
		private CorrelationData TimedScopeData { get; set; }


		/// <summary>
		/// Constructs the timed scope correlation data
		/// </summary>
		/// <returns>Correlation data</returns>
		private CorrelationData ConstructCorrelationDataEntries(IMachineInformation machineInformation)
		{

			// TODO what is the difference between CrossCutting.Corelation.CurrentCorelation and TimedScopeData
			CorrelationData correlationData = TimedScopeData;

			CorrelationData scopeData = TimedScopeData.Clone();

			scopeData.AddData(TimedScopeDataKeys.InternalOnly.ScopeName, Name);
			scopeData.AddData(TimedScopeDataKeys.InternalOnly.InstanceId, InstanceId.ToString());
			scopeData.AddData(TimedScopeDataKeys.InternalOnly.IsSuccessful, IsSuccessful.HasValue ? IsSuccessful.Value.ToString() : bool.FalseString);
			scopeData.AddData(TimedScopeDataKeys.InternalOnly.IsRoot, IsRoot.ToString());
			scopeData.AddData(TimedScopeDataKeys.InternalOnly.ScopeResult, Result.ToString());

			bool isFailed = !IsSuccessful ?? false;
			if (isFailed && FailureDescription != null)
			{
				scopeData.AddData(TimedScopeDataKeys.InternalOnly.FailureDescription, FailureDescription.ToString());
			}

			scopeData.AddData(TimedScopeDataKeys.InternalOnly.Duration, DurationInMilliseconds.ToString(CultureInfo.InvariantCulture));
			long sequenceNumber = correlationData == null ? 0 : correlationData.NextEventSequenceNumber();
			scopeData.AddData(TimedScopeDataKeys.InternalOnly.SequenceNumber, sequenceNumber.ToString(CultureInfo.InvariantCulture));
			scopeData.AddData(TimedScopeDataKeys.InternalOnly.CallDepth, correlationData == null ? "0" : correlationData.CallDepth.ToString(CultureInfo.InvariantCulture));

			// TODO aks
			IMachineInformation machineInfo = machineInformation;

			if (machineInfo != null)
			{
				scopeData.AddData(TimedScopeDataKeys.InternalOnly.MachineId, machineInfo.MachineId);
				scopeData.AddData(TimedScopeDataKeys.InternalOnly.MachineCluster, machineInfo.MachineCluster);
				scopeData.AddData(TimedScopeDataKeys.InternalOnly.MachineRole, machineInfo.MachineRole);
				scopeData.AddData(TimedScopeDataKeys.InternalOnly.AgentName, machineInfo.AgentName);
			}

			// if the user hash has been set, add it to the scope data
			if (!string.IsNullOrWhiteSpace(m_userHashOverride))
			{
				ULSLogging.LogTraceTag(0x2385029e /* tag_97qk4 */, Categories.TimingGeneral, Levels.Verbose,
					"Overriding user hash metadata in the Timed Scope '{0}' with value '{1}'", Name, m_userHashOverride);
				scopeData.AddData(TimedScopeDataKeys.InternalOnly.UserHash, m_userHashOverride);
			}
			else if (correlationData != null && !string.IsNullOrWhiteSpace(correlationData.UserHash))
			{
				scopeData.AddData(TimedScopeDataKeys.InternalOnly.UserHash, correlationData.UserHash);
			}

			// capture performance metrics
			if (PerfDiagnostics != null && PerfDiagnostics.LastStatus)
			{
				scopeData.AddData(TimedScopeDataKeys.InternalOnly.CpuCycles,
					PerfDiagnostics.CyclesUsed.ToString(CultureInfo.InvariantCulture));
				scopeData.AddData(TimedScopeDataKeys.InternalOnly.UserModeDuration,
					PerfDiagnostics.UserModeMilliseconds.ToString(CultureInfo.InvariantCulture));
				scopeData.AddData(TimedScopeDataKeys.InternalOnly.KernelModeDuration,
					PerfDiagnostics.KernelModeMilliseconds.ToString(CultureInfo.InvariantCulture));
				scopeData.AddData(TimedScopeDataKeys.InternalOnly.HttpRequestCount,
					PerfDiagnostics.HttpRequestCount.ToString(CultureInfo.InvariantCulture));
				scopeData.AddData(TimedScopeDataKeys.InternalOnly.ServiceCallCount,
					PerfDiagnostics.ServiceCallCount.ToString(CultureInfo.InvariantCulture));
			}

			return scopeData;
		}


		/// <summary>
		/// Retrieve or create a guid which identifies set of all active timedscopes for this flow
		/// </summary>
		private static Guid? AllScopesGuid
		{
			get
			{
				Context.ICallContext callContext = new ThreadCallContext().ExistingCallContext();

				if (callContext != null)
				{
					Guid scopesId;
					object scopesIdObject;
					if (callContext.SharedData.TryGetValue(AllActiveScopesDataKey, out scopesIdObject))
					{
						scopesId = (Guid)scopesIdObject;
					}
					else
					{
						scopesId = Guid.NewGuid();
						callContext.SharedData[AllActiveScopesDataKey] = scopesId;
					}

					return scopesId;
				}

				return null;
			}
		}


		/// <summary>
		/// Get a stack of active scopes, creating a new stack if one does not exist
		/// </summary>
		/// <returns>stack of scopes</returns>
		private static TimedScopeStack Scopes
		{
			get
			{
				Context.ICallContext callContext = new ThreadCallContext().ExistingCallContext();

				TimedScopeStack stack = null;
				if (callContext != null)
				{
					object stackObject = null;
					if (callContext.Data.TryGetValue(ActiveScopesDataKey, out stackObject))
					{
						stack = stackObject as TimedScopeStack;
					}

					if (stack == null)
					{
						stack = TimedScopeStack.Root;
						callContext.Data[ActiveScopesDataKey] = stack;
					}
				}

				return stack;
			}
			set
			{
				Context.ICallContext callContext = new ThreadCallContext().ExistingCallContext();

				if (callContext != null)
				{
					callContext.Data[ActiveScopesDataKey] = value;
				}
			}
		}


		/// <summary>
		/// Modify the set of active scopes
		/// </summary>
		/// <param name="addScope">should the scope be added or removed</param>
		private void ModifyActiveScopes(bool addScope)
		{
			TimedScopeStack scopes = Scopes;
			if (scopes == null)
			{
				return;
			}

			if (addScope)
			{
				scopes = scopes.Push(this);
			}
			else
			{
				Stack<TimedScope> temporaryScopes = new Stack<TimedScope>();
				while (!scopes.IsRoot)
				{
					TimedScope popScope;
					scopes = scopes.Pop(out popScope);
					if (ReferenceEquals(popScope, this))
					{
						break;
					}

					temporaryScopes.Push(popScope);
				}

				while (temporaryScopes.Count > 0)
				{
					scopes = scopes.Push(temporaryScopes.Pop());
				}
			}

			Scopes = scopes;
		}


		/// <summary>
		/// Converts bool? scope results (legacy type) to TimedScopeResult type
		/// </summary>
		/// <param name="scopeResult">Scope result</param>
		/// <returns>Scope result of type TimedScopeResult</returns>
		public static TimedScopeResult ConvertBoolResultToTimedScopeResult(bool? scopeResult)
		{
			if (!scopeResult.HasValue)
			{
				return default(TimedScopeResult);
			}
			else if (scopeResult.Value)
			{
				return TimedScopeResult.Success;
			}
			else
			{
				return TimedScopeResult.SystemError;
			}
		}


		/// <summary>
		/// LinkedStack
		/// </summary>
		[Serializable]
		private class TimedScopeStack
		{
			/// <summary>
			/// Root item for all stacks
			/// </summary>
			public static TimedScopeStack Root { get; } = new TimedScopeStack();


			/// <summary>
			/// Root node
			/// </summary>
			public bool IsRoot => ReferenceEquals(this, Parent);


			/// <summary>
			/// Adds a new item to the stack
			/// </summary>
			/// <param name="item">Data item to store</param>
			/// <returns>Stack with the new item on it</returns>
			public TimedScopeStack Push(TimedScope item) => new TimedScopeStack(item, this);


			/// <summary>
			/// Remove item from the stack and return the new stack
			/// </summary>
			/// <param name="scope">TimedScope stored at the top od the stack</param>
			/// <returns>New stack with the top item removed</returns>
			public TimedScopeStack Pop(out TimedScope scope)
			{
				scope = Item;
				return Parent;
			}


			/// <summary>
			/// Retrieve item from the top of the stack
			/// </summary>
			/// <returns></returns>
			public TimedScope Peek() => Item;


			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="item">Item stored in the stack</param>
			/// <param name="parent">Parent of this stack</param>
			private TimedScopeStack(TimedScope item, TimedScopeStack parent)
			{
				Code.ExpectsArgument(parent, nameof(parent), TaggingUtilities.ReserveTag(0));

				Item = item;
				Parent = parent;
			}


			/// <summary>
			/// Constructor
			/// </summary>
			private TimedScopeStack()
			{
				Parent = this;
			}


			/// <summary>
			/// Parent of this node
			/// </summary>
			private TimedScopeStack Parent { get; }


			/// <summary>
			/// Data item stored in this node
			/// </summary>
			private TimedScope Item { get; }
		}


		/// <summary>
		/// Data key used to store the active time scopes on the call context
		/// </summary>
		private const string ActiveScopesDataKey = "TimedScope.ActiveScopes";


		/// <summary>
		/// Data key used to store set of all active time scopes on the call context
		/// </summary>
		private const string AllActiveScopesDataKey = "TimedScope.AllActiveScopes";


		/// <summary>
		/// Internal failure descriptions
		/// </summary>
		private enum InternalFailureDescription
		{
			/// <summary>
			/// Failure description set when we change unknown error to system error
			/// </summary>
			UnknownResultAsSystemError,
		}

		/// <summary>
		/// Running transaction Id, or Transactions.None
		/// </summary>
		public uint RunningTransaction { get; private set; }
	}
}
