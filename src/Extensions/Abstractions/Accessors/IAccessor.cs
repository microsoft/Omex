// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Provides access to object that is not available during dependency injection contaniner build,
	/// because it could be not available during container creation
	/// </summary>
	public interface IAccessor<out TValue>
		where TValue : class
	{
		/// <summary>
		/// Get value if it's available
		/// </summary>
		TValue? Value { get; }

		/// <summary>
		/// Execute function when value is available
		/// </summary>
		/// <remarks>
		/// If value is not null action will be executed immediately,
		/// otherwise activity will be stored until it's available and then executed.
		/// </remarks>
		void OnAvailable(Action<TValue> function);
	}
}
