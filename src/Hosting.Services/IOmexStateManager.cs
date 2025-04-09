// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Interface for managing the state of a Service Fabric replica.
	/// </summary>
	public interface IOmexStateManager
	{
		/// <summary>
		/// Gets the current role of the replica.
		/// </summary>
		/// <returns>The role of the replica.</returns>
		ReplicaRole GetRole();

		/// <summary>
		/// Gets a value indicating whether the state manager is readable.
		/// </summary>
		bool IsReadable { get; }

		/// <summary>
		/// Gets a value indicating whether the state manager is writable.
		/// </summary>
		bool IsWritable { get; }

		/// <summary>
		/// Gets the reliable state manager instance.
		/// </summary>
		IReliableStateManager State { get; }
	}
}
