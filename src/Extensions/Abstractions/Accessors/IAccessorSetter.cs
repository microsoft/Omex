// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Hosting.Services
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
		/// Beside saving value it will also execute saved activities that where waiting for a value
		/// </remarks>
		void SetContext(TValue value);
	}
}
