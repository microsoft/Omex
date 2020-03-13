// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.Tracing;
using Microsoft.Omex.Extensions.EventSources;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Service Fabric event source
	/// </summary>
	[EventSource(Name = "Microsoft-OMEX-ServiceInitializationLogs")] //Breaking Change: event source renamed
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
		public void LogServiceTypeRegistered(int hostProcessId, string serviceType)
		{
			if (!IsEnabled())
			{
				return;
			}

			LogServiceTypeRegistered(
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
		public void LogServiceHostInitializationFailed(string exception, string serviceType)
		{
			if (!IsEnabled())
			{
				return;
			}

			LogServiceHostInitializationFailed(
				exception,
				serviceType,
				FormattableString.Invariant($"Service host initialization failed for {serviceType} with exception {exception}"));
		}


		private ServiceInitializationEventSource() { }


		[Event((int)EventSourcesEventIds.ServiceTypeRegistered, Level = EventLevel.Informational, Message = "{2}", Version = 1)]
		private void LogServiceTypeRegistered(int hostProcessId, string serviceType, string message) =>
			WriteEvent((int)EventSourcesEventIds.ServiceTypeRegistered, hostProcessId, serviceType, message);


		//Breaking Change: serviceType paramiter added
		[Event((int)EventSourcesEventIds.ServiceHostInitializationFailed, Level = EventLevel.Error, Message = "{1}", Version = 1)]
		private void LogServiceHostInitializationFailed(string exception, string serviceType, string message) =>
			WriteEvent((int)EventSourcesEventIds.ServiceHostInitializationFailed, exception, serviceType, message);
	}
}
