// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.Tracing;
using Microsoft.Omex.Extensions.Abstractions.EventSources;

namespace Microsoft.Omex.Extensions.Logging
{
	// Renamed from Microsoft-OMEX-Logs to avoid conflict with sources in other libraries
	[EventSource(Name = "Microsoft-OMEX-Logs-Ext")] //TODO: new event source should be registered GitHub Issue #187
	internal sealed class OmexLogEventSource : EventSource
	{
		[Event((int)EventSourcesEventIds.LogError, Level = EventLevel.Error, Message = "{12}:{13} {16}", Version = 6)]
		public void LogErrorServiceMessage(
			string applicationName,
			string serviceName,
			string agentName,
			string buildVersion,
			string processName,
			Guid partitionId,
			long replicaId,
			string activityId,
			string activityTraceId,
			Guid correlationId,
			uint transactionId,
			string level,
			string category,
			string tagId,
			string tagName,
			int threadId,
			string message) =>
			WriteEvent((int)EventSourcesEventIds.LogError, applicationName, serviceName, agentName, buildVersion, processName, partitionId, replicaId,
				activityId, activityTraceId, correlationId, transactionId, level, category, tagId, tagName, threadId, message);

		[Event((int)EventSourcesEventIds.LogWarning, Level = EventLevel.Warning, Message = "{12}:{13} {16}", Version = 6)]
		public void LogWarningServiceMessage(
			string applicationName,
			string serviceName,
			string agentName,
			string buildVersion,
			string processName,
			Guid partitionId,
			long replicaId,
			string activityId,
			string activityTraceId,
			Guid correlationId,
			uint transactionId,
			string level,
			string category,
			string tagId,
			string tagName,
			int threadId,
			string message) =>
			WriteEvent((int)EventSourcesEventIds.LogWarning, applicationName, serviceName, agentName, buildVersion, processName, partitionId, replicaId,
				activityId, activityTraceId, correlationId, transactionId, level, category, tagId, tagName, threadId, message);

		[Event((int)EventSourcesEventIds.LogInfo, Level = EventLevel.Informational, Message = "{12}:{13} {16}", Version = 6)]
		public void LogInfoServiceMessage(
			string applicationName,
			string serviceName,
			string agentName,
			string buildVersion,
			string processName,
			Guid partitionId,
			long replicaId,
			string activityId,
			string activityTraceId,
			Guid correlationId,
			uint transactionId,
			string level,
			string category,
			string tagId,
			string tagName,
			int threadId,
			string message) =>
			WriteEvent((int)EventSourcesEventIds.LogInfo, applicationName, serviceName, agentName, buildVersion, processName, partitionId, replicaId,
				activityId, activityTraceId, correlationId, transactionId, level, category, tagId, tagName, threadId, message);

		[Event((int)EventSourcesEventIds.LogVerbose, Level = EventLevel.Verbose, Message = "{12}:{13} {16}", Version = 6)]
		public void LogVerboseServiceMessage(
			string applicationName,
			string serviceName,
			string agentName,
			string buildVersion,
			string processName,
			Guid partitionId,
			long replicaId,
			string activityId,
			string activityTraceId,
			Guid correlationId,
			uint transactionId,
			string level,
			string category,
			string tagId,
			string tagName,
			int threadId,
			string message) =>
			WriteEvent((int)EventSourcesEventIds.LogVerbose, applicationName, serviceName, agentName, buildVersion, processName, partitionId, replicaId,
				activityId, activityTraceId, correlationId, transactionId, level, category, tagId, tagName, threadId, message);

		[Event((int)EventSourcesEventIds.LogSpam, Level = EventLevel.Verbose, Message = "{12}:{13} {16}", Version = 6)]
		public void LogSpamServiceMessage(
			string applicationName,
			string serviceName,
			string agentName,
			string buildVersion,
			string processName,
			Guid partitionId,
			long replicaId,
			string activityId,
			string activityTraceId,
			Guid correlationId,
			uint transactionId,
			string level,
			string category,
			string tagId,
			string tagName,
			int threadId,
			string message) =>
			WriteEvent((int)EventSourcesEventIds.LogSpam, applicationName, serviceName, agentName, buildVersion, processName, partitionId, replicaId,
				activityId, activityTraceId, correlationId, transactionId, level, category, tagId, tagName, threadId, message);

		public static OmexLogEventSource Instance { get; } = new OmexLogEventSource();

		private OmexLogEventSource() { }
	}
}
