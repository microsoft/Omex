// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Abstractions.Activities.Processing
{
	/// <summary>
	/// Provides activities
	/// </summary>
	/// <remarks>This interface would be deleted after move to net 5.0, since inheritance won't be a supported extension model for Activity</remarks>
	public interface IActivityProvider
	{
		/// <summary>
		/// Creates activity instance
		/// </summary>
		/// <param name="definition">Timed scope definition</param>
		/// <returns>Newly created activity</returns>
		Activity Create(TimedScopeDefinition definition);
	}
}
