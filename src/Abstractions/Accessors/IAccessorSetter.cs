// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Abstractions.Accessors
{
	/// <summary>
	/// Interface to provide value for accessor
	/// </summary>
	public interface IAccessorSetter<TValue>
		where TValue : class
	{
		/// <summary>
		/// Set value when it's available
		/// </summary>
		/// <remarks>
		/// Beside saving value it will also execute saved activities that were waiting for a value
		/// </remarks>
		void SetValue(TValue value);
	}
}
