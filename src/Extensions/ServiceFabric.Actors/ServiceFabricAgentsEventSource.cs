// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.Tracing;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.ServiceFabric
{
	/// <summary>
	/// Service Fabric event source
	/// </summary>
	[EventSource(Name = "Microsoft-OMEX-Logs")] //Breaking Change: duplicated name, might need to be renamed
	internal sealed class ServiceFabricAgentsEventSource : EventSource
	{
		/// <summary>
		/// Instance of service fabric event source
		/// </summary>
		public static ServiceFabricAgentsEventSource Instance { get; } = new ServiceFabricAgentsEventSource();


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


		private ServiceFabricAgentsEventSource() { }


		[Event((int)EventSourcesEventIds.ActorTypeRegisteredEventId, Level = EventLevel.Informational, Message = "{2}", Version = 1)]
		private void LogActorTypeRegistered(int hostProcessId, string actorType, string message) =>
			WriteEvent((int)EventSourcesEventIds.ActorTypeRegisteredEventId, hostProcessId, actorType, message);


		[Event((int)EventSourcesEventIds.ActorHostInitializationFailedEventId, Level = EventLevel.Error, Message = "{1}", Version = 1)]
		private void LogActorHostInitializationFailed(string exception, string message) =>
			WriteEvent((int)EventSourcesEventIds.ActorHostInitializationFailedEventId, exception, message);
	}
}
