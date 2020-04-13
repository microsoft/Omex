// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Creates comunication listener for SF service
	/// </summary>
	public interface IListenerBuilder<in TContext>
		where TContext : ServiceContext
	{
		/// <summary>
		/// Listener name
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Creates comunication listener for SF service
		/// </summary>
		ICommunicationListener Build(TContext service);
	}
}
