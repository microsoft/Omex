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
		/// Returns value from accessor or throws <see cref="InvalidOperationException" /> if it's null
		/// </summary>
		/// <exception cref="InvalidOperationException">If value is null</exception>
		TValue GetValueOrThrow();

		/// <summary>
		/// Execute function when value is available (set in the first time)
		/// </summary>
		/// <remarks>
		/// If value is not null, action will be executed immediately,
		/// otherwise activity will be stored until it's available and then executed,
		/// list of actions would be cleared after execution
		/// </remarks>
		void OnFirstSet(Action<TValue> function);
	}
}
