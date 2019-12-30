// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.Tracing;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.ServiceFabric
{
	/// <summary>
	/// Service Fabric event source
	/// </summary>
	[EventSource(Name = "Microsoft-OMEX-Logs-SfInit")] //Breaking Change: Name Changed from "Microsoft-OMEX-Logs"
	internal sealed class ServiceFabricEventSource : EventSource
	{
		/// <summary>
		/// Instance of service fabric event source
		/// </summary>
		public static ServiceFabricEventSource Instance { get; } = new ServiceFabricEventSource();


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


		/// <summary>
		/// Logs a service type registered ETW event.
		/// </summary>
		/// <param name="hostProcessId">Host process id</param>
		/// <param name="actorType">Actor type</param>
		[NonEvent]
		public void LogActorTypeRegistered(int hostProcessId, string actorType)
		{
			if (!IsEnabled())
			{
				return;
			}

			LogActorTypeRegistered(hostProcessId, actorType, $"Actor host process {hostProcessId} registered actor type {actorType}");
		}


		/// <summary>
		/// Logs an actor host initialization failed ETW event.
		/// </summary>
		/// <param name="exception">Exception</param>
		[NonEvent]
		public void LogActorHostInitializationFailed(string exception)
		{
			if (!IsEnabled())
			{
				return;
			}

			LogActorHostInitializationFailed(exception, "Actor host initialization failed");
		}


		private ServiceFabricEventSource() { }


		[Event((int)EventIds.ServiceTypeRegisteredEventId, Level = EventLevel.Informational, Message = "{2}", Version = 1)]
		private void LogServiceTypeRegistered(int hostProcessId, string serviceType, string message) =>
			WriteEvent((int)EventIds.ServiceTypeRegisteredEventId, hostProcessId, serviceType, message);


		[Event((int)EventIds.ServiceHostInitializationFailedEventId, Level = EventLevel.Error, Message = "{1}", Version = 1)]
		private void LogServiceHostInitializationFailed(string exception, string message) =>
			WriteEvent((int)EventIds.ServiceHostInitializationFailedEventId, exception, message);


		[Event((int)EventIds.ActorTypeRegisteredEventId, Level = EventLevel.Informational, Message = "{2}", Version = 1)]
		private void LogActorTypeRegistered(int hostProcessId, string actorType, string message) =>
			WriteEvent((int)EventIds.ActorTypeRegisteredEventId, hostProcessId, actorType, message);


		[Event((int)EventIds.ActorHostInitializationFailedEventId, Level = EventLevel.Error, Message = "{1}", Version = 1)]
		private void LogActorHostInitializationFailed(string exception, string message) =>
			WriteEvent((int)EventIds.ActorHostInitializationFailedEventId, exception, message);
	}
}
