// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>
	/// Provides activities
	/// </summary>
	public interface IActivityProvider
	{
		/// <summary>
		/// Create activity instance
		/// </summary>
		/// <param name="operationName">The name of the operation</param>
		/// <returns>New started activity</returns>
		Activity Create(string operationName);
	}
}
