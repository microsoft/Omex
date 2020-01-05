// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>Event ids enum</summary>
	public enum EventSourcesEventIds : ushort
	{
		/// <summary>Event Id for logging general info message</summary>
		LogInfoEventId = 1,


		/// <summary>Event Id for logging error message</summary>
		LogErrorEventId = 2,


		/// <summary>Event Id for logging warning message</summary>
		LogWarningEventId = 3,


		/// <summary>Event Id for logging verbose message</summary>
		LogVerboseEventId = 4,


		/// <summary>Event Id for logging spam message</summary>
		LogSpamEventId = 5,


		/// <summary>Event Id for logging timed scopes</summary>
		LogTimedScopeEventId = 6,


		/// <summary>Event Id for logging timed scopes traces</summary>
		LogTimedScopeTestContextEventId = 7,


		/// <summary>Event Id for service type registered</summary>
		ServiceTypeRegisteredEventId = 11,


		/// <summary>Event Id for service host initialization failed</summary>
		ServiceHostInitializationFailedEventId = 12,


		/// <summary>Event Id for actor type registered</summary>
		ActorTypeRegisteredEventId = 13,


		/// <summary>Event Id for actor host initialization failed</summary>
		ActorHostInitializationFailedEventId = 14,


		/// <summary>Event Id for logging Analytics information</summary>
		LogAnalyticsEvent = 15,
	}
}
