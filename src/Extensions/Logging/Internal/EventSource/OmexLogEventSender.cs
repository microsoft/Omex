// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
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


		public OmexLogEventSender(OmexLogEventSource eventSource, IExecutionContext machineInformation, IServiceContext context)
		{
			m_eventSource = eventSource;
			m_machineInformation = machineInformation;
			m_serviceContext = context;
		}


		public void LogMessage(Activity activity, string category, LogLevel level, EventId eventId, int threadId, string message)
		{
			if (!IsEnabled(level))
			{
				return;
			}

			string activityId = activity?.Id ?? string.Empty;
			ActivityTraceId traceId = activity?.TraceId ?? default;
			Guid partitionId = m_serviceContext.PartitionId;
			long replicaId = m_serviceContext.ReplicaOrInstanceId;
			string applicationName = m_machineInformation.ApplicationName;
			string serviceName = m_machineInformation.ServiceName;
			string buildVersion = m_machineInformation.BuildVersion;
			string machineId = m_machineInformation.MachineId;

			string tagName = eventId.Name;
			// In case if tag created using Tag.Create (line number and file in description) it's better to display decimal number 
			string tagId = string.IsNullOrWhiteSpace(eventId.Name)
				? TagsExtensions.TagIdAsString(eventId.Id)
				: eventId.Id.ToString(CultureInfo.InvariantCulture);

			string traceIdAsString = traceId.ToHexString();

			//Event methods should have all information as parameters so we are passing them each time
			// Posible Breaking changes:
			// 1. CorrelationId type changed from Guid ?? Guid.Empty
			// 2. TransactionId type Changed from uint ?? 0u
			// 3. ThreadId type Changed from string
			// 4. TagName to events so it should be also send
			switch (level)
			{
				case LogLevel.None:
					break;
				case LogLevel.Trace:
					m_eventSource.LogSpamServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId, activityId, traceIdAsString, "Spam", category, tagId, tagName, threadId, message);
					break;
				case LogLevel.Debug:
					m_eventSource.LogVerboseServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId, activityId, traceIdAsString, "Verbose", category, tagId, tagName, threadId, message);
					break;
				case LogLevel.Information:
					m_eventSource.LogInfoServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId, activityId, traceIdAsString, "Info", category, tagId, tagName, threadId, message);
					break;
				case LogLevel.Warning:
					m_eventSource.LogWarningServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId, activityId, traceIdAsString, "Warning", category, tagId, tagName, threadId, message);
					break;
				case LogLevel.Error:
				case LogLevel.Critical:
					m_eventSource.LogErrorServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId, activityId, traceIdAsString, "Error", category, tagId, eventId.Name, threadId, message);
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
		private readonly IExecutionContext m_machineInformation;
		private static readonly string s_processName;
	}
}
