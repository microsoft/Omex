// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.Omex.Extensions.Abstractions.Option;

namespace Microsoft.Omex.Extensions.Activities
{
	internal sealed class ActivityEventSender : IActivitiesEventSender
	{
		public ActivityEventSender(ActivityEventSource eventSource, IExecutionContext executionContext, ILogger<ActivityEventSender> logger, IOptions<MonitoringOption> options)
		{
			m_eventSource = eventSource;
			m_serviceName = executionContext.ServiceName;
			m_logger = logger;
			m_options = options;
		}

		public void SendActivityMetric(Activity activity)
		{
			if (m_options.Value.DisableEventSender || !m_eventSource.IsEnabled())
			{
				return;
			}

			string serviceName = m_serviceName;
			string activityId = activity.Id ?? string.Empty;
			string name = activity.OperationName;
			double durationMs = activity.Duration.TotalMilliseconds;
			string userHash = activity.GetUserHash();
			bool isHealthCheck = activity.IsHealthCheck();

			string subtype = NullPlaceholder;
			string metadata = NullPlaceholder;
			string resultAsString = NullPlaceholder;
			foreach (KeyValuePair<string, string?> pair in activity.Tags)
			{
				if (pair.Value == null)
				{
					continue;
				}

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

#pragma warning disable CS0618 // Until it's used we need to include correlationId into events
			string correlationId = activity.GetObsoleteCorrelationId()?.ToString()
				?? activity.GetRootIdAsGuid()?.ToString()
				?? NullPlaceholder;
#pragma warning restore CS0618

			string nameAsString = SanitizeString(name, nameof(name), name);
			string subTypeAsString = SanitizeString(subtype, nameof(subtype), name);
			string metaDataAsString = SanitizeString(metadata, nameof(metadata), name);
			string userHashAsString = SanitizeString(userHash, nameof(userHash), name);
			string serviceNameAsString = SanitizeString(serviceName, nameof(serviceName), name);
			string correlationIdAsString = SanitizeString(correlationId, nameof(correlationId), name);
			string activityIdAsString = SanitizeString(activityId, nameof(activityId), name);
			long durationMsAsLong = Convert.ToInt64(durationMs, CultureInfo.InvariantCulture);

			if (isHealthCheck)
			{
				m_eventSource.WriteTimedScopeTestEvent(
					name: nameAsString,
					subType: subTypeAsString,
					metadata: metaDataAsString,
					serviceName: serviceNameAsString,
					logCategory: s_logCategory,
					result: resultAsString,
					correlationId: correlationIdAsString,
					activityId: activityIdAsString,
					durationMs: durationMsAsLong);
			}
			else
			{
				m_eventSource.WriteTimedScopeEvent(
					name: nameAsString,
					subType: subTypeAsString,
					metadata: metaDataAsString,
					userHash: userHashAsString,
					serviceName: serviceNameAsString,
					logCategory: s_logCategory,
					result: resultAsString,
					correlationId: correlationIdAsString,
					activityId: activityIdAsString,
					durationMs: durationMsAsLong);
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
			"Log aggregation enforces a string length limit of {0} characters per dimension. Truncating length of dimension {1} on activity {2} from {3} chars in order to allow upload of the metric";

		private readonly ActivityEventSource m_eventSource;
		private readonly string m_serviceName;
		private readonly ILogger<ActivityEventSender> m_logger;
		private readonly IOptions<MonitoringOption> m_options;
		private static readonly string s_logCategory = typeof(ActivityEventSource).FullName ?? nameof(ActivityEventSource);
		private const string NullPlaceholder = "null";
	}
}
