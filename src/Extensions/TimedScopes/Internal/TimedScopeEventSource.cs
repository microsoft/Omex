// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.TimedScopes
{


	[EventSource(Name = "Microsoft-OMEX-TimedScopes")]
	internal sealed class TimedScopeEventSource : EventSource, ITimedScopeEventSource
	{
		public TimedScopeEventSource(IMachineInformation machineInformation, ILogger<TimedScopeEventSource> logger)
		{
			m_serviceName = machineInformation.ServiceName;
			m_logger = logger;
			m_logCategory = typeof(TimedScopeEventSource).FullName ?? nameof(TimedScopeEventSource);
		}


		[NonEvent]
		public void LogTimedScopeEndEvent(TimedScope scope)
		{
			if (!IsEnabled())
			{
				return;
			}

			string serviceName = m_serviceName;
			string subtype = scope.SubType;
			string metadata = scope.MetaData;
			TimedScopeResult result = scope.Result;
			Activity activity = scope.Activity;
			string name = activity.OperationName;
			string correlationId = activity.Id;
			double durationMs = activity.Duration.TotalMilliseconds;
			string userHash = activity.GetUserHash(); //Breaking Change: feild not set
			bool isTransaction = activity.IsTransaction(); //Breaking Change: feild not set

			string nameAsString = SanitizeString(name, nameof(name), name);
			string subTypeAsString = SanitizeString(subtype, nameof(subtype), name);
			string metaDataAsString = SanitizeString(metadata, nameof(metadata), name);
			string userHashAsString = SanitizeString(userHash, nameof(userHash), name);
			string serviceNameAsString = SanitizeString(serviceName, nameof(serviceName), name);
			string correlationIdAsString = SanitizeString(correlationId, nameof(correlationId), name);
			string resultAsString = result.ToString();
			long durationMsAsLong = Convert.ToInt64(durationMs, CultureInfo.InvariantCulture);

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


		private string SanitizeString(string value, string name, string activityName)
		{
			const int stringLimit = 1024;
			if (value.Length > stringLimit)
			{
				m_logger.LogWarning(Tag.ReserveTag(0), StringLimitMessage, stringLimit, name, activityName, value.Length);
				value = value.Substring(0, stringLimit);
			}

			return value;
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


		private readonly string m_serviceName;
		private readonly string m_logCategory;
		private readonly ILogger<TimedScopeEventSource> m_logger;
	}
}
