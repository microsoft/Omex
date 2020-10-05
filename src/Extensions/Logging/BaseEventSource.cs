using System;
using System.Diagnostics.Tracing;
using Microsoft.Omex.Extensions.Abstractions.EventSources;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// 
	/// </summary>
	public class BaseEventSource : EventSource
	{
		/// <summary>
		/// 
		/// </summary>
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

		/// <summary>
		/// 
		/// </summary>
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

		/// <summary>
		/// 
		/// </summary>
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

		/// <summary>
		/// 
		/// </summary>
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

		/// <summary>
		/// 
		/// </summary>
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
	}
}
