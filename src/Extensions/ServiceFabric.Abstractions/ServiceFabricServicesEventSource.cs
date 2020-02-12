// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.Tracing;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.ServiceFabric.Services
{
	/// <summary>
	/// Service Fabric event source
	/// </summary>
	[EventSource(Name = "Microsoft-OMEX-Logs")] //Breaking Change: duplicated name, might need to be renamed
	internal sealed class ServiceFabricServicesEventSource : EventSource
	{
		/// <summary>
		/// Instance of service fabric event source
		/// </summary>
		public static ServiceFabricServicesEventSource Instance { get; } = new ServiceFabricServicesEventSource();


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

			LogServiceTypeRegistered(hostProcessId, serviceType, $"Service host process {hostProcessId} registered service type {serviceType}");
		}


		/// <summary>
		/// Logs a service host initialization failed ETW event.
		/// </summary>
		/// <param name="exception">Exception</param>
		[NonEvent]
		public void LogServiceHostInitializationFailed(string exception)
		{
			if (!IsEnabled())
			{
				return;
			}

			LogServiceHostInitializationFailed(exception, "Service host initialization failed");
		}


		private ServiceFabricServicesEventSource() { }


		[Event((int)EventSourcesEventIds.ServiceTypeRegisteredEventId, Level = EventLevel.Informational, Message = "{2}", Version = 1)]
		private void LogServiceTypeRegistered(int hostProcessId, string serviceType, string message) =>
			WriteEvent((int)EventSourcesEventIds.ServiceTypeRegisteredEventId, hostProcessId, serviceType, message);


		[Event((int)EventSourcesEventIds.ServiceHostInitializationFailedEventId, Level = EventLevel.Error, Message = "{1}", Version = 1)]
		private void LogServiceHostInitializationFailed(string exception, string message) =>
			WriteEvent((int)EventSourcesEventIds.ServiceHostInitializationFailedEventId, exception, message);
	}
}
