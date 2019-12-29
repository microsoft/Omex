// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Logging.TimedScopes;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Timed scopes event source
	/// </summary>
	[EventSource(Name = "Microsoft-OMEX-TimedScopes")]
	internal sealed class TimedScopeEventSource : EventSource
	{
		/// <summary>
		/// Create TimedScopeEventSource
		/// </summary>
		public TimedScopeEventSource(ILogger<TimedScopeEventSource> logger)
		{
			m_logger = logger;
			m_logCategory = typeof(TimedScopeEventSource).FullName!;
		}


		/// <summary>
		/// Logs the occurrence of an activity
		/// </summary>
		/// <param name="name">TimedScope name</param>
		/// <param name="subtype">TimedScope subtype</param>
		/// <param name="metadata">TimedScope metadata</param>
		/// <param name="userHash">User hash</param>
		/// <param name="serviceName">Service name</param>
		/// <param name="result">TimedScope result</param>
		/// <param name="correlationId">Correlation Id</param>
		/// <param name="durationMs">TimedScope duration in milliseconds</param>
		/// <param name="isTransaction">Is it transaction event or not</param>
		[NonEvent]
		public void LogEvent(
				string name,
				string subtype,
				string metadata,
				string userHash,
				string serviceName,
				TimedScopeResult result,
				string correlationId,
				double durationMs,
				bool isTransaction)
		{
			if (!IsEnabled())
			{
				return;
			}

			string nameAsString = SanitizeString(name, nameof(name), name);
			string subTypeAsString = SanitizeString(subtype, nameof(subtype), name);
			string metaDataAsString = SanitizeString(metadata, nameof(metadata), name);
			string userHashAsString = SanitizeString(userHash, nameof(userHash), name);
			string serviceNameAsString = SanitizeString(serviceName, nameof(serviceName), name);
			string correlationIdAsString = SanitizeString(correlationId, nameof(correlationId), name);
			string resultAsString = result.ToString();
			long durationMsAsLong = Convert.ToInt64(durationMs);

			if (isTransaction)
			{
				WriteTimedScopeTrxEvent(
					nameAsString,
					subTypeAsString,
					metaDataAsString,
					serviceNameAsString,
					resultAsString,
					correlationIdAsString,
					durationMsAsLong);
			}
			else
			{
				WriteTimedScopeEvent(
					nameAsString,
					subTypeAsString,
					metaDataAsString,
					userHashAsString,
					serviceNameAsString,
					resultAsString,
					correlationIdAsString,
					durationMsAsLong);
			}
		}


		private string SanitizeString(string mdmString, string name, string activityName)
		{
			string validatedString = Convert.ToString(mdmString) ?? string.Empty;

			const int stringLimit = 1024;
			if (validatedString.Length > stringLimit)
			{
				m_logger.LogWarning(TaggingUtilities.ReserveTag(0), StringLimitMessage, stringLimit, name, activityName, validatedString.Length);
				validatedString = validatedString.Substring(0, stringLimit);
			}

			return validatedString;
		}


		private const string StringLimitMessage =
			"Log aggregator enforces a string length limit of {0} characters per dimension. Truncating length of dimension {1} on activity {2} from {3} chars in order to allow upload of the metric";


		[Event((int)EventIds.LogTimedScopeEventId, Level = EventLevel.Informational, Version = 3)]
		private void WriteTimedScopeEvent(
			string name,
			string subType,
			string metadata,
			string userHash,
			string serviceName,
			string result,
			string correlationId,
			long durationMs) =>
			WriteEvent((int)EventIds.LogTimedScopeEventId, name, subType, metadata, userHash, serviceName, m_logCategory, result, correlationId, durationMs);


		[Event((int)EventIds.LogTimedScopeTrxEventId, Level = EventLevel.Informational, Version = 3)]
		private void WriteTimedScopeTrxEvent(
			string name,
			string subType,
			string metadata,
			string serviceName,
			string result,
			string correlationId,
			long durationMs) =>
			WriteEvent((int)EventIds.LogTimedScopeTrxEventId, name, subType, metadata, serviceName, m_logCategory, result, correlationId, durationMs);


		private readonly ILogger<TimedScopeEventSource> m_logger;


		private readonly string m_logCategory;
	}
}
