// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Provides access to service fabric service contest
	/// </summary>
	public interface IServiceContextAccessor<out TServiceContext>
		where TServiceContext : ServiceContext
	{
		/// <summary>
		/// Get service context if it's available
		/// </summary>
		TServiceContext? ServiceContext { get; }

		/// <summary>
		/// Execute function when context value is available
		/// </summary>
		void OnContextAvailable(Action<TServiceContext> function);
	}
}
