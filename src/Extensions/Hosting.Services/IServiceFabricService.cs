// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Interface to generalize service fabric services
	/// </summary>
	public interface IServiceFabricService<out TContext>
		where TContext : ServiceContext
	{
		/// <summary>
		/// Gets the service context that this service is operating under
		/// </summary>
		TContext Context { get; }
	}
}
