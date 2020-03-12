// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Abstractions.ReplayableLogs
{
	/// <summary>
	/// Provides activities
	/// </summary>
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
