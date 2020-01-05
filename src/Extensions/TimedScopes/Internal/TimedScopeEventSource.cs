// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Logging.TimedScopes;

namespace Microsoft.Omex.Extensions.Logging
{
	[EventSource(Name = "Microsoft-OMEX-TimedScopes")]
	internal sealed class TimedScopeEventSource : EventSource
	{
		public TimedScopeEventSource(ILogger<TimedScopeEventSource> logger)
		{
			m_logger = logger;
			m_logCategory = typeof(TimedScopeEventSource).FullName ?? nameof(TimedScopeEventSource);
		}


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
				WriteTimedScopeTestEvent(
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
				m_logger.LogWarning(Tag.ReserveTag(0), StringLimitMessage, stringLimit, name, activityName, validatedString.Length);
				validatedString = validatedString.Substring(0, stringLimit);
			}

			return validatedString;
		}


		private const string StringLimitMessage =
			"Log aggregator enforces a string length limit of {0} characters per dimension. Truncating length of dimension {1} on activity {2} from {3} chars in order to allow upload of the metric";


		[Event((int)EventSourcesEventIds.LogTimedScopeEventId, Level = EventLevel.Informational, Version = 3)]
		private void WriteTimedScopeEvent(
			string name,
			string subType,
			string metadata,
			string userHash,
			string serviceName,
			string result,
			string correlationId,
			long durationMs) =>
			WriteEvent((int)EventSourcesEventIds.LogTimedScopeEventId, name, subType, metadata, userHash, serviceName, m_logCategory, result, correlationId, durationMs);


		[Event((int)EventSourcesEventIds.LogTimedScopeTestContextEventId , Level = EventLevel.Informational, Version = 3)]
		private void WriteTimedScopeTestEvent(
			string name,
			string subType,
			string metadata,
			string serviceName,
			string result,
			string correlationId,
			long durationMs) =>
			WriteEvent((int)EventSourcesEventIds.LogTimedScopeTestContextEventId , name, subType, metadata, serviceName, m_logCategory, result, correlationId, durationMs);


		private readonly ILogger<TimedScopeEventSource> m_logger;
		private readonly string m_logCategory;
	}
}
