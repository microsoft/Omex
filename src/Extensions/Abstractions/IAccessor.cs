// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>
	/// Provides access to an object that is not available during dependency injection container build,
	/// because it was not available during container creation
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
		/// If value is not null, action will be executed immediately,
		/// otherwise activity will be stored until it's available and then executed.
		/// </remarks>
		void OnUpdated(Action<TValue> function);
	}
}
