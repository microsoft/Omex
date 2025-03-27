// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Abstractions.EventSources
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
		/// Event Id for logging activities
		/// </summary>
		LogActivity = 6,

		/// <summary>
		/// Event Id for logging test activities
		/// </summary>
		LogActivityTestContext = 7,
	}
}
