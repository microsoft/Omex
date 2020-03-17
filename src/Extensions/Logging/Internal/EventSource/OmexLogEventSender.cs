// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Logging.Replayable;

namespace Microsoft.Omex.Extensions.Logging
{
	internal sealed class OmexLogEventSender : ILogEventSender, ILogEventReplayer
	{
		static OmexLogEventSender()
		{
			Process process = Process.GetCurrentProcess();
			s_processName = string.Format(CultureInfo.InvariantCulture, "{0} (0x{1:X4})", process.ProcessName, process.Id);
		}


		public OmexLogEventSender(OmexLogEventSource eventSource, IExecutionContext executionContext, IServiceContext context, IOptions<OmexLoggingOptions> options)
		{
			m_eventSource = eventSource;
			m_executionContext = executionContext;
			m_serviceContext = context;
			m_options = options;
		}


		public void LogMessage(Activity? activity, string category, LogLevel level, EventId eventId, int threadId, string message)
		{
			if (!IsEnabled(level))
			{
				return;
			}

			Guid partitionId = m_serviceContext.PartitionId;
			long replicaId = m_serviceContext.ReplicaOrInstanceId;
			string applicationName = m_executionContext.ApplicationName;
			string serviceName = m_executionContext.ServiceName;
			string buildVersion = m_executionContext.BuildVersion;
			string machineId = m_executionContext.MachineId;

			string tagName = eventId.Name;
			// In case if tag created using Tag.Create (line number and file in description) it's better to display decimal number 
			string tagId = string.IsNullOrWhiteSpace(eventId.Name)
#pragma warning disable CS0618 // Need to be used for to process reserved tags from GitTagger
				? TagsExtensions.TagIdAsString(eventId.Id)
#pragma warning restore CS0618
				: eventId.Id.ToString(CultureInfo.InvariantCulture);


			string activityId = string.Empty;
			ActivityTraceId activityTraceId = default;
			Guid obsoleteCorrelationId = Guid.Empty;
			uint obsoleteTransactionId = 0u;
			if (activity != null)
			{
				activityId = activity.Id ?? string.Empty;
				activityTraceId = activity.TraceId;

				if (m_options.Value.AddObsoleteCorrelationToActivity)
				{
#pragma warning disable CS0618 // We are using obsolete correlation to support logging correlation from old Omex services
					obsoleteCorrelationId = activity.GetObsoleteCorrelationId() ?? Guid.Empty;
					obsoleteTransactionId = activity.GetObsolteteTransactionId() ?? 0u;
#pragma warning restore CS0618
				}
			}

			string traceIdAsString = activityTraceId.ToHexString();


			//Event methods should have all information as parameters so we are passing them each time
			// Posible Breaking changes:
			// 1. ThreadId type Changed from string to avoid useless string creation
			// 2. New fileds added:
			//  a. tagName to events since it will have more useful information
			//  b. activityId required for tracking net core activity
			//  c. activityTraceId required for tracking net core activity
			switch (level)
			{
				case LogLevel.None:
					break;
				case LogLevel.Trace:
					m_eventSource.LogSpamServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId,
						activityId, traceIdAsString, obsoleteCorrelationId, obsoleteTransactionId, "Spam", category, tagId, tagName, threadId, message);
					break;
				case LogLevel.Debug:
					m_eventSource.LogVerboseServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId,
						activityId, traceIdAsString, obsoleteCorrelationId, obsoleteTransactionId, "Verbose", category, tagId, tagName, threadId, message);
					break;
				case LogLevel.Information:
					m_eventSource.LogInfoServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId,
						activityId, traceIdAsString, obsoleteCorrelationId, obsoleteTransactionId, "Info", category, tagId, tagName, threadId, message);
					break;
				case LogLevel.Warning:
					m_eventSource.LogWarningServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId,
						activityId, traceIdAsString, obsoleteCorrelationId, obsoleteTransactionId, "Warning", category, tagId, tagName, threadId, message);
					break;
				case LogLevel.Error:
				case LogLevel.Critical:
					m_eventSource.LogErrorServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId,
						activityId, traceIdAsString, obsoleteCorrelationId, obsoleteTransactionId, "Error", category, tagId, eventId.Name, threadId, message);
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


		public bool IsReplayableMessage(LogLevel logLevel) =>
			logLevel switch
			{
				LogLevel.Trace => true,
				LogLevel.Debug => true,
				_ => false
			};


		public void ReplayLogs(Activity activity)
		{
			// Replay parent activity
			if (activity.Parent != null)
			{
				ReplayLogs(activity.Parent);
			}

			if (activity is ReplayableActivity replayableActivity)
			{
				foreach (LogMessageInformation log in replayableActivity.GetLogEvents())
				{
					LogMessage(activity, log.Category, LogLevel.Information, log.EventId, log.ThreadId, log.Message);
				}
			}
		}


		private readonly OmexLogEventSource m_eventSource;
		private readonly IServiceContext m_serviceContext;
		private readonly IOptions<OmexLoggingOptions> m_options;
		private readonly IExecutionContext m_executionContext;
		private static readonly string s_processName;
	}
}
