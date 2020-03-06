// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.Tracing;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	[EventSource(Name = "Microsoft-OMEX-TimedScopes")]
	internal sealed class TimedScopeEventSource : EventSource
	{
		public static TimedScopeEventSource Instance { get; } = new TimedScopeEventSource();


		[Event((int)EventSourcesEventIds.LogTimedScope, Level = EventLevel.Informational, Version = 3)]
		public void WriteTimedScopeEvent(
			string name,
			string subType,
			string metadata,
			string userHash,
			string serviceName,
			string result,
			string correlationId,
			long durationMs) =>
			WriteEvent((int)EventSourcesEventIds.LogTimedScope, name, subType, metadata, userHash, serviceName, m_logCategory, result, correlationId, durationMs);


		[Event((int)EventSourcesEventIds.LogTimedScopeTestContext, Level = EventLevel.Informational, Version = 3)]
		public void WriteTimedScopeTestEvent(
			string name,
			string subType,
			string metadata,
			string serviceName,
			string result,
			string correlationId,
			long durationMs) =>
			WriteEvent((int)EventSourcesEventIds.LogTimedScopeTestContext, name, subType, metadata, serviceName, m_logCategory, result, correlationId, durationMs);


		private TimedScopeEventSource() =>
			m_logCategory = typeof(TimedScopeEventSource).FullName ?? nameof(TimedScopeEventSource);


		private readonly string m_logCategory;
	}
}
