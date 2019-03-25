// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.Omex.System.Context
{
	/// <summary>
	/// Context data specific to the current call
	/// </summary>
	public interface ICallContext : ICallContextProvider
	{
		/// <summary>
		/// Dictionary of data stored on the call context
		/// </summary>
		IDictionary<string, object> Data { get; }


		/// <summary>
		/// Dictionary of data stored on the call context and shared between derived call contexts
		/// </summary>
		ConcurrentDictionary<string, object> SharedData { get; }


		/// <summary>
		/// Add data to context
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="value">value</param>
		void AddContextValue(string key, object value);
	}
}
