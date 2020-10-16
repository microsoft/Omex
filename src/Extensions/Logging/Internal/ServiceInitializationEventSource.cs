﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.Tracing;
using Microsoft.Omex.Extensions.Abstractions.EventSources;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Service Fabric event source
	/// </summary>
	[EventSource(Name = "Microsoft-OMEX-HostLogs")] //TODO: new event source should be registred GitHub Issue #187
	internal sealed class ServiceInitializationEventSource : EventSource
	{
		/// <summary>
		/// Instance of service fabric event source
		/// </summary>
		public static ServiceInitializationEventSource Instance { get; } = new ServiceInitializationEventSource();

		/// <summary>
		/// Logs a generic host build success
		/// </summary>
		/// <param name="hostProcessId">Host process id</param>
		/// <param name="serviceType">Service type</param>
		/// <param name="message">Optional message</param>
		[NonEvent]
		public void LogHostBuildSucceeded(int hostProcessId, string serviceType, string message = "")
		{
			if (!IsEnabled())
			{
				return;
			}

			string logMessage = string.IsNullOrWhiteSpace(message) ?
				FormattableString.Invariant($"Service host process {hostProcessId} registered service type {serviceType}") :
				message;
			LogHostBuildSucceededInternal(
				hostProcessId,
				serviceType,
				logMessage);
		}

		/// <summary>
		/// Logs a generic host failure
		/// </summary>
		/// <param name="exception">Exception</param>
		/// <param name="serviceType">Service type</param>
		/// <param name="message">Optional message</param>
		[NonEvent]
		public void LogHostFailed(string exception, string serviceType, string message = "")
		{
			if (!IsEnabled())
			{
				return;
			}
			string logMessage = string.IsNullOrWhiteSpace(message) ?
				FormattableString.Invariant($"Service host initialization failed for {serviceType} with exception {exception}") :
				message;

			LogHostFailedInternal(
				exception,
				serviceType,
				message);
		}

		private ServiceInitializationEventSource() { }

		/// <summary>
		/// Log host build succeeded
		/// </summary>
		/// <param name="hostProcessId">Host process id</param>
		/// <param name="serviceType">The service type</param>
		/// <param name="message">The message to be logged</param>
		[Event((int)EventSourcesEventIds.GenericHostBuildSucceeded, Level = EventLevel.Informational, Message = "{2}", Version = 1)]
		private void LogHostBuildSucceededInternal(int hostProcessId, string serviceType, string message) =>
			WriteEvent((int)EventSourcesEventIds.GenericHostBuildSucceeded, hostProcessId, serviceType, message);

		/// <summary>
		/// Log host build failed
		/// </summary>
		/// <param name="exception">Exception to be logged</param>
		/// <param name="serviceType">The service type</param>
		/// <param name="message">The message to be logged</param>
		[Event((int)EventSourcesEventIds.GenericHostFailed, Level = EventLevel.Error, Message = "{1}", Version = 1)]
		private void LogHostFailedInternal(string exception, string serviceType, string message) =>
			WriteEvent((int)EventSourcesEventIds.GenericHostFailed, exception, serviceType, message);
	}
}
