// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Startup
{
	/// <summary>Create wrapper from Service Fabric context to provide log information</summary>
	public class OmexServiceFabricContext : OmexServiceContext
	{
		/// <summary> Create OmexServiceFabricContext from StatelessServiceContext</summary>
		public OmexServiceFabricContext(StatelessServiceContext context)
			: base(context.PartitionId, context.ReplicaOrInstanceId)
		{
		}


		/// <summary> Create OmexServiceFabricContext from StatefulServiceContext</summary>
		public OmexServiceFabricContext(StatefulServiceContext context)
			: base(context.PartitionId, context.ReplicaOrInstanceId)
		{
		}
	}
}
