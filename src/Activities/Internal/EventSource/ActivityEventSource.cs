// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.Tracing;
using Microsoft.Omex.Extensions.Abstractions.EventSources;

namespace Microsoft.Omex.Extensions.Activities
{
	// Renamed from Microsoft-OMEX-TimedScopes to avoid conflict with sources in other libraries
	[EventSource(Name = "Microsoft-OMEX-TimedScopes-Ext")]
	internal sealed class ActivityEventSource : EventSource
	{
		[Event((int)EventSourcesEventIds.LogActivity, Level = EventLevel.Informational, Version = 4)]
		public void WriteTimedScopeEvent(
			string name,
			string subType,
			string metadata,
			string userHash,
			string serviceName,
			string logCategory,
			string result,
			string correlationId,
			string activityId,
			long durationMs,
			string dependent) =>
			WriteEvent((int)EventSourcesEventIds.LogActivity, name, subType, metadata, userHash, serviceName, logCategory, result, correlationId, activityId, durationMs, dependent);

		[Event((int)EventSourcesEventIds.LogActivityTestContext, Level = EventLevel.Informational, Version = 4)]
		public void WriteTimedScopeTestEvent(
			string name,
			string subType,
			string metadata,
			string serviceName,
			string logCategory,
			string result,
			string correlationId,
			string activityId,
			long durationMs,
			string dependent) =>
			WriteEvent((int)EventSourcesEventIds.LogActivityTestContext, name, subType, metadata, serviceName, logCategory, result, correlationId, activityId, durationMs, dependent);

		public static ActivityEventSource Instance { get; } = new ActivityEventSource();
	}
}
