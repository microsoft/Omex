/***************************************************************************************************
	TimedScopeEventSource.cs

	Copyright (c) Microsoft Corporation.

	Timed scopes event source
***************************************************************************************************/

using System;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Omex.System.Logging;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Timed scopes event source
	/// </summary>
	[EventSource(Name = "Microsoft-OMEX-TimedScopes")]
	public sealed class TimedScopeEventSource : EventSource
	{
		#region Initialization

		/// <summary>
		/// An initialized instance of the <see cref="TimedScopeEventSource"/> class
		/// The singleton behavior is needed so that the Event Source works properly
		/// </summary>
		public static TimedScopeEventSource Instance => s_instance.Value;


		/// <summary>
		/// Static constructor
		/// </summary>
		static TimedScopeEventSource()
		{
			// A workaround for the problem where ETW activities do not get tracked until Tasks infrastructure is initialized.
			// This problem will be fixed in .NET Framework 4.6.2.
			Task.Run(() => { });
		}


		/// <summary>
		/// Private constructor to enforce singleton semantics needed for the Event Source to work properly
		/// </summary>
		private TimedScopeEventSource()
			: base()
		{
		}


		/// <summary>
		/// Lazy instance of the <see cref="TimedScopeEventSource"/> class
		/// </summary>
		private static readonly Lazy<TimedScopeEventSource> s_instance = new Lazy<TimedScopeEventSource>(
			() => new TimedScopeEventSource(),
			LazyThreadSafetyMode.PublicationOnly);

		#endregion


		#region Non-Events

		/// <summary>
		/// Logs the occurrence of an activity
		/// </summary>
		/// <param name="logCategory">Log category</param>
		/// <param name="name">TimedScope name</param>
		/// <param name="subtype">TimedScope subtype</param>
		/// <param name="metadata">TimedScope metadata</param>
		/// <param name="userHash">User hash</param>
		/// <param name="serviceName">Service name</param>
		/// <param name="result">TimedScope result</param>
		/// <param name="correlationId">Correlation Id</param>
		/// <param name="durationMs">TimedScope duration in milliseconds</param>
		[NonEvent]
		public void LogEvent(
				Category logCategory,
				string name,
				string subtype,
				string metadata,
				string userHash,
				string serviceName,
				TimedScopeResult result,
				string correlationId,
				double durationMs)
		{
			string nameAsString = SanitizeString(name, nameof(name), name, logCategory);
			string subTypeAsString = SanitizeString(subtype, nameof(subtype), name, logCategory);
			string metaDataAsString = SanitizeString(metadata, nameof(metadata), name, logCategory);
			string userHashAsString = SanitizeString(userHash, nameof(userHash), name, logCategory);
			string serviceNameAsString = SanitizeString(serviceName, nameof(serviceName), name, logCategory);
			string correlationIdAsString = SanitizeString(correlationId, nameof(correlationId), name, logCategory);

			WriteTimedScopeEvent(
				nameAsString,
				subTypeAsString,
				metaDataAsString,
				userHashAsString,
				serviceNameAsString,
				logCategory.Name,
				result.ToString(),
				correlationIdAsString,
				Convert.ToInt64(durationMs, CultureInfo.InvariantCulture));
		}


		/// <summary>
		/// Logs the occurrence of an activity
		/// </summary>
		/// <param name="logCategory">Log category</param>
		/// <param name="name">TimedScope name</param>
		/// <param name="subtype">TimedScope subtype</param>
		/// <param name="metadata">TimedScope metadata</param>
		/// <param name="serviceName">Service name</param>
		/// <param name="result">TimedScope result</param>
		/// <param name="correlationId">Correlation Id</param>
		/// <param name="durationMs">TimedScope duration in milliseconds</param>
		[NonEvent]
		public void LogEvent(
			Category logCategory,
			string name,
			string subtype,
			string metadata,
			string serviceName,
			TimedScopeResult result,
			string correlationId,
			double durationMs)
		{
			string nameAsString = SanitizeString(name, nameof(name), name, logCategory);
			string subTypeAsString = SanitizeString(subtype, nameof(subtype), name, logCategory);
			string metaDataAsString = SanitizeString(metadata, nameof(metadata), name, logCategory);
			string serviceNameAsString = SanitizeString(serviceName, nameof(serviceName), name, logCategory);
			string correlationIdAsString = SanitizeString(correlationId, nameof(correlationId), name, logCategory);

			WriteTimedScopeTestContextEvent(
				nameAsString,
				subTypeAsString,
				metaDataAsString,
				serviceNameAsString,
				logCategory.Name,
				result.ToString(),
				correlationIdAsString,
				Convert.ToInt64(durationMs, CultureInfo.InvariantCulture));
		}


		private static string SanitizeString(string mdmString, string name, string activityName, Category logCategory)
		{
			string validatedString = Convert.ToString(mdmString, CultureInfo.InvariantCulture) ?? string.Empty;

			if (validatedString.Length > 1024)
			{
				ULSLogging.LogTraceTag(0x23857681 /* tag_97x0b */, logCategory, Levels.Warning, StringLimitMessage, 1024, name, activityName, validatedString.Length);
				validatedString = validatedString.Substring(0, 1024);
			}

			return validatedString;
		}


		private const string StringLimitMessage =
			"Our logging enforces a string length limit of {0} characters per dimension. Truncating length of dimension {1} on activity {2} from {3} chars in order to allow upload of the metric";

		#endregion


		#region Events

		/// <summary>
		/// Writes a Timed scope event with Level=Informational.
		/// </summary>
		/// <param name="name">Timed scope name</param>
		/// <param name="subType">Subtype</param>
		/// <param name="metadata">Metadata</param>
		/// <param name="serviceName">Service name</param>
		/// <param name="userHash">User hash</param>
		/// <param name="logCategory">Log category</param>
		/// <param name="result">Result</param>
		/// <param name="correlationId">Correlation Id</param>
		/// <param name="durationMs">Duration in ms</param>
		[Event((int)EventIds.LogTimedScopeEventId, Level = EventLevel.Informational, Version = 3)]
		private void WriteTimedScopeEvent(
			string name,
			string subType,
			string metadata,
			string userHash,
			string serviceName,
			string logCategory,
			string result,
			string correlationId,
			long durationMs)
		{
			WriteEvent((int)EventIds.LogTimedScopeEventId, name ?? string.Empty, subType ?? string.Empty, metadata ?? string.Empty, userHash ?? string.Empty, serviceName ?? string.Empty, logCategory, result, correlationId, durationMs);
		}


		/// <summary>
		/// Writes a Timed scope trace event with Level=Informational.
		/// </summary>
		/// <param name="name">Timed scope name</param>
		/// <param name="subType">Subtype</param>
		/// <param name="metadata">Metadata</param>
		/// <param name="serviceName">Service name</param>
		/// <param name="logCategory">Log category</param>
		/// <param name="result">Result</param>
		/// <param name="correlationId">Correlation Id</param>
		/// <param name="durationMs">Duration in ms</param>
		[Event((int)EventIds.LogTimedScopeTestContextEventId, Level = EventLevel.Informational, Version = 3)]
		private void WriteTimedScopeTestContextEvent(
			string name,
			string subType,
			string metadata,
			string serviceName,
			string logCategory,
			string result,
			string correlationId,
			long durationMs)
		{
			WriteEvent((int)EventIds.LogTimedScopeTestContextEventId, name ?? string.Empty, subType ?? string.Empty, metadata ?? string.Empty, serviceName ?? string.Empty, logCategory, result, correlationId, durationMs);
		}

		#endregion
	}
}
