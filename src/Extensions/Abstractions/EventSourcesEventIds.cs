// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.EventSources // diffirent namespace since this class has specific usage
{
	/// <summary>
	/// Event ids enum
	/// </summary>
	/// <remarks>
	/// enum numbers preserve historical order, changing them will require to change our log aggregation system
	/// </remarks>
	public enum EventSourcesEventIds
	{
		/// <summary>
		/// Event Id for logging general info message
		/// </summary>
		LogInfo = 1,


		/// <summary>
		/// Event Id for logging error message
		/// </summary>
		LogError = 2,


		/// <summary>
		/// Event Id for logging warning message
		/// </summary>
		LogWarning = 3,


		/// <summary>
		/// Event Id for logging verbose message
		/// </summary>
		LogVerbose = 4,


		/// <summary>
		/// Event Id for logging spam message
		/// </summary>
		LogSpam = 5,


		/// <summary>
		/// Event Id for logging timed scopes
		/// </summary>
		LogTimedScope = 6,


		/// <summary>
		/// Event Id for logging timed scopes traces
		/// </summary>
		LogTimedScopeTestContext = 7,


		/// <summary>
		/// Event Id for service type registered
		/// </summary>
		ServiceTypeRegistered = 11,


		/// <summary>
		/// Event Id for service host initialization failed
		/// </summary>
		ServiceHostInitializationFailed = 12,


		/// <summary>
		/// Event Id for actor type registered
		/// </summary>
		ActorTypeRegistered = 13,


		/// <summary>
		/// Event Id for actor host initialization failed
		/// </summary>
		ActorHostInitializationFailed = 14,


		/// <summary>
		/// Event Id for logging Analytics information
		/// </summary>
		LogAnalytics = 15,
	}
}
