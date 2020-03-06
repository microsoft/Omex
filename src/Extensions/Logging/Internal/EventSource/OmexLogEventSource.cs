// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.Tracing;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Logging
{
	[EventSource(Name = "Microsoft-OMEX-Logs")]
	internal sealed class OmexLogEventSource : EventSource
	{

		public static OmexLogEventSource Instance { get; } = new OmexLogEventSource();


		[Event((int)EventSourcesEventIds.LogError, Level = EventLevel.Error, Message = "{13}", Version = 6)]
		public void LogErrorServiceMessage(
			string applicationName,
			string serviceName,
			string agentName,
			string buildVersion,
			string processName,
			Guid partitionId,
			long replicaId,
			string correlationId,
			string transactionId,
			string level,
			string category,
			string tagId,
			string tagName,
			int threadId,
			string message) =>
			WriteEvent((int)EventSourcesEventIds.LogError, applicationName, serviceName, agentName, buildVersion, processName, partitionId, replicaId, correlationId, transactionId, level, category, tagId, tagName, threadId, message);


		[Event((int)EventSourcesEventIds.LogWarning, Level = EventLevel.Warning, Message = "{13}", Version = 6)]
		public void LogWarningServiceMessage(
			string applicationName,
			string serviceName,
			string agentName,
			string buildVersion,
			string processName,
			Guid partitionId,
			long replicaId,
			string correlationId,
			string transactionId,
			string level,
			string category,
			string tagId,
			string tagName,
			int threadId,
			string message) =>
			WriteEvent((int)EventSourcesEventIds.LogWarning, applicationName, serviceName, agentName, buildVersion, processName, partitionId, replicaId, correlationId, transactionId, level, category, tagId, tagName, threadId, message);


		[Event((int)EventSourcesEventIds.LogInfo, Level = EventLevel.Informational, Message = "{13}", Version = 6)]
		public void LogInfoServiceMessage(
			string applicationName,
			string serviceName,
			string agentName,
			string buildVersion,
			string processName,
			Guid partitionId,
			long replicaId,
			string correlationId,
			string transactionId,
			string level,
			string category,
			string tagId,
			string tagName,
			int threadId,
			string message) =>
			WriteEvent((int)EventSourcesEventIds.LogInfo, applicationName, serviceName, agentName, buildVersion, processName, partitionId, replicaId, correlationId, transactionId, level, category, tagId, tagName, threadId, message);


		[Event((int)EventSourcesEventIds.LogVerbose, Level = EventLevel.Verbose, Message = "{13}", Version = 6)]
		public void LogVerboseServiceMessage(
			string applicationName,
			string serviceName,
			string agentName,
			string buildVersion,
			string processName,
			Guid partitionId,
			long replicaId,
			string correlationId,
			string transactionId,
			string level,
			string category,
			string tagId,
			string tagName,
			int threadId,
			string message) =>
			WriteEvent((int)EventSourcesEventIds.LogVerbose, applicationName, serviceName, agentName, buildVersion, processName, partitionId, replicaId, correlationId, transactionId, level, category, tagId, tagName, threadId, message);


		[Event((int)EventSourcesEventIds.LogSpam, Level = EventLevel.Verbose, Message = "{13}", Version = 6)]
		public void LogSpamServiceMessage(
			string applicationName,
			string serviceName,
			string agentName,
			string buildVersion,
			string processName,
			Guid partitionId,
			long replicaId,
			string correlationId,
			string transactionId,
			string level,
			string category,
			string tagId,
			string tagName,
			int threadId,
			string message) =>
			WriteEvent((int)EventSourcesEventIds.LogSpam, applicationName, serviceName, agentName, buildVersion, processName, partitionId, replicaId, correlationId, transactionId, level, category, tagId, tagName, threadId, message);


		private OmexLogEventSource() { }
	}
}
