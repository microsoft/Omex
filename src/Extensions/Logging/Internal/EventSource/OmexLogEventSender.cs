﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;

namespace Microsoft.Omex.Extensions.Logging
{
	internal sealed class OmexLogEventSender : ILogEventSender
	{
		static OmexLogEventSender()
		{
			Process process = Process.GetCurrentProcess();
			s_processName = string.Format(CultureInfo.InvariantCulture, "{0} (0x{1:X4})", process.ProcessName, process.Id);
		}

		public OmexLogEventSender(OmexLogEventSource eventSource, IExecutionContext executionContext, IServiceContext context, IOptionsMonitor<OmexLoggingOptions> options)
		{
			m_eventSource = eventSource;
			m_executionContext = executionContext;
			m_serviceContext = context;
			m_options = options;
		}

		public void LogMessage(Activity? activity, string category, LogLevel level, EventId eventId, int threadId, string message, Exception? exception)
		{
			if (!IsEnabled(level))
			{
				return;
			}

			// NOTE: Currently, we're not doing anything with the exception as the message when an exception is logged will already contain the exception details.
			// However, in the future, it's possible we might want to log details, such as exception type or exception message, in separate columns.

			Guid partitionId = m_serviceContext.PartitionId;
			long replicaId = m_serviceContext.ReplicaOrInstanceId;
			string applicationName = m_executionContext.ApplicationName;
			string serviceName = m_executionContext.ServiceName;
			string buildVersion = m_executionContext.BuildVersion;
			string machineId = m_executionContext.MachineId;

			string tagName = eventId.Name ?? string.Empty;
			// In case if tag created using Tag.Create (line number and file in description) it's better to display decimal number
			string tagId = string.IsNullOrWhiteSpace(eventId.Name)
				? eventId.ToTagId()
				: eventId.Id.ToString(CultureInfo.InvariantCulture);

			string activityId = string.Empty;
			ActivityTraceId activityTraceId = default;
			Guid obsoleteCorrelationId = Guid.Empty;
			uint obsoleteTransactionId = 0u;
			bool isHealthCheck = false;
			if (activity != null)
			{
				activityId = activity.Id ?? string.Empty;
				activityTraceId = activity.TraceId;
				isHealthCheck = activity.IsHealthCheck();

				if (m_options.CurrentValue.AddObsoleteCorrelationToActivity)
				{
#pragma warning disable CS0618 // We are using obsolete correlation to support logging correlation from old Omex services
					obsoleteCorrelationId = activity.GetObsoleteCorrelationId() ?? activity.GetRootIdAsGuid() ?? Guid.Empty;
					obsoleteTransactionId = activity.GetObsoleteTransactionId() ?? 0u;
#pragma warning restore CS0618
				}
			}

			string traceIdAsString = activityTraceId.ToHexString();

			//Event methods should have all information as parameters so we are passing them each time
			// Possible Breaking changes:
			// 1. ThreadId type Changed from string to avoid useless string creation
			// 2. New fields added:
			//  a. tagName to events since it will have more useful information
			//  b. activityId required for tracking net core activity
			//  c. activityTraceId required for tracking net core activity
			switch (level)
			{
				case LogLevel.None:
					break;
				case LogLevel.Trace:
					m_eventSource.LogSpamServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId,
						activityId, traceIdAsString, obsoleteCorrelationId, obsoleteTransactionId, "Spam", category, tagId, tagName, threadId, message, isHealthCheck);
					break;
				case LogLevel.Debug:
					m_eventSource.LogVerboseServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId,
						activityId, traceIdAsString, obsoleteCorrelationId, obsoleteTransactionId, "Verbose", category, tagId, tagName, threadId, message, isHealthCheck);
					break;
				case LogLevel.Information:
					m_eventSource.LogInfoServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId,
						activityId, traceIdAsString, obsoleteCorrelationId, obsoleteTransactionId, "Info", category, tagId, tagName, threadId, message, isHealthCheck);
					break;
				case LogLevel.Warning:
					m_eventSource.LogWarningServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId,
						activityId, traceIdAsString, obsoleteCorrelationId, obsoleteTransactionId, "Warning", category, tagId, tagName, threadId, message, isHealthCheck);
					break;
				case LogLevel.Error:
				case LogLevel.Critical:
					m_eventSource.LogErrorServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId,
						activityId, traceIdAsString, obsoleteCorrelationId, obsoleteTransactionId, "Error", category, tagId, tagName, threadId, message, isHealthCheck);
					break;
				default:
					throw new ArgumentException(FormattableString.Invariant($"Unknown EventLevel: {level}"));
			}
		}

		public bool IsEnabled(LogLevel level) =>
			level switch
			{
				LogLevel.None => false,
				_ => m_eventSource.IsEnabled()
			};

		private readonly OmexLogEventSource m_eventSource;
		private readonly IServiceContext m_serviceContext;
		private readonly IOptionsMonitor<OmexLoggingOptions> m_options;
		private readonly IExecutionContext m_executionContext;
		private static readonly string s_processName;
	}
}
