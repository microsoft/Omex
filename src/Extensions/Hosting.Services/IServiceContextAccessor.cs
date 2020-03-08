// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Provides access to service fabric service context
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
		/// <remarks>
		/// If service context is not null action will be executed immediately,
		/// otherwise activity will be stored until context available and then executed.
		/// It should be used when you want to update state bases values from ServiceContext when it's available.
		/// Please restrain from calling context properties multiple times and consider saving their values instead.
		/// </remarks>
		void OnContextAvailable(Action<TServiceContext> function);
	}
}
