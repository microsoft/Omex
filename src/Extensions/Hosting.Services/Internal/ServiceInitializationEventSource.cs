// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.Tracing;
using Microsoft.Omex.Extensions.Abstractions.EventSources;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Service Fabric event source
	/// </summary>
	[EventSource(Name = "Microsoft-OMEX-ServiceInitializationLogs")] //TODO: new event source should be registred GitHub Issue #187
	internal sealed class ServiceInitializationEventSource : EventSource
	{
		/// <summary>
		/// Instance of service fabric event source
		/// </summary>
		public static ServiceInitializationEventSource Instance { get; } = new ServiceInitializationEventSource();

		/// <summary>
		/// Logs a service type registered ETW event.
		/// </summary>
		/// <param name="hostProcessId">Host process id</param>
		/// <param name="serviceType">Service type</param>
		[NonEvent]
		public void LogHostBuildSucceded(int hostProcessId, string serviceType)
		{
			if (!IsEnabled())
			{
				return;
			}

			LogHostBuildeSucceded(
				hostProcessId,
				serviceType,
				FormattableString.Invariant($"Service host process {hostProcessId} registered service type {serviceType}"));
		}

		/// <summary>
		/// Logs a service host initialization failed ETW event.
		/// </summary>
		/// <param name="exception">Exception</param>
		/// <param name="serviceType">Service type</param>
		[NonEvent]
		public void LogHostBuildFailed(string exception, string serviceType)
		{
			if (!IsEnabled())
			{
				return;
			}

			LogHostBuildFailed(
				exception,
				serviceType,
				FormattableString.Invariant($"Service host initialization failed for {serviceType} with exception {exception}"));
		}

		private ServiceInitializationEventSource() { }

		[Event((int)EventSourcesEventIds.GenericHostBuildSucceded, Level = EventLevel.Informational, Message = "{2}", Version = 1)]
		private void LogHostBuildeSucceded(int hostProcessId, string serviceType, string message) =>
			WriteEvent((int)EventSourcesEventIds.GenericHostBuildSucceded, hostProcessId, serviceType, message);

		[Event((int)EventSourcesEventIds.GenericHostBuildFailed, Level = EventLevel.Error, Message = "{1}", Version = 1)]
		private void LogHostBuildFailed(string exception, string serviceType, string message) =>
			WriteEvent((int)EventSourcesEventIds.GenericHostBuildFailed, exception, serviceType, message);
	}
}
