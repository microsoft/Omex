// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
		/// Service fabric service context
		/// </summary>
		TServiceContext? ServiceContext { get; }
	}
}
