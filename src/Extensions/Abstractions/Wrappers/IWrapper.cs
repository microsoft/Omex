// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Abstractions.Wrappers
{
	/// <summary>
	/// Generic Wrapper for objects
	/// </summary>
	public interface IWrapper<T>
	{
		/// <summary>
		/// Returns stored object from wrapper
		/// </summary>
		public Task<T> GetAsync();
	}
}
