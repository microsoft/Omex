// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	internal sealed class TimedScopeEventSender : ITimedScopeEventSender
	{
		public TimedScopeEventSender(TimedScopeEventSource eventSource, IHostEnvironment hostEnvironment, ILogger<TimedScopeEventSender> logger)
		{
			m_eventSource = eventSource;
			m_serviceName = hostEnvironment.ApplicationName;
			m_logger = logger;
		}


		public void LogActivityStop(Activity activity)
		{
			if (!m_eventSource.IsEnabled())
			{
				return;
			}

			string serviceName = m_serviceName;
			string name = activity.OperationName;
			string correlationId = activity.Id;
			double durationMs = activity.Duration.TotalMilliseconds;
			string userHash = activity.GetUserHash(); //TODO: We need add middleware that will set userhash in compliant way and IsTransaction GitHub Issue #166
			bool isTransaction = activity.IsTransaction();


			string subtype = NullPlaceholder;
			string metadata = NullPlaceholder;
			string resultAsString = NullPlaceholder;
			foreach (KeyValuePair<string, string> pair in activity.Tags)
			{
				if (string.Equals(ActivityTagKeys.Result, pair.Key, StringComparison.Ordinal))
				{
					resultAsString = pair.Value;
				}
				else if (string.Equals(ActivityTagKeys.SubType, pair.Key, StringComparison.Ordinal))
				{
					subtype = pair.Value;
				}
				else if (string.Equals(ActivityTagKeys.Metadata, pair.Key, StringComparison.Ordinal))
				{
					metadata = pair.Value;
				}
			}

			string nameAsString = SanitizeString(name, nameof(name), name);
			string subTypeAsString = SanitizeString(subtype, nameof(subtype), name);
			string metaDataAsString = SanitizeString(metadata, nameof(metadata), name);
			string userHashAsString = SanitizeString(userHash, nameof(userHash), name);
			string serviceNameAsString = SanitizeString(serviceName, nameof(serviceName), name);
			string correlationIdAsString = SanitizeString(correlationId, nameof(correlationId), name);
			long durationMsAsLong = Convert.ToInt64(durationMs, CultureInfo.InvariantCulture);

			if (isTransaction)
			{
				m_eventSource.WriteTimedScopeTestEvent(
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
				m_eventSource.WriteTimedScopeEvent(
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
				m_logger.LogWarning(Tag.Create(), StringLimitMessage, stringLimit, name, activityName, value.Length);
				value = value.Substring(0, stringLimit);
			}

			return value;
		}


		private const string StringLimitMessage =
			"Log aggregator enforces a string length limit of {0} characters per dimension. Truncating length of dimension {1} on activity {2} from {3} chars in order to allow upload of the metric";


		private readonly TimedScopeEventSource m_eventSource;
		private readonly string m_serviceName;
		private readonly ILogger<TimedScopeEventSender> m_logger;
		private const string NullPlaceholder = "null";
	}
}
