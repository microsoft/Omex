// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>Information about service context</summary>
	public abstract class OmexServiceContext : IServiceContext
	{
		/// <summary>Creates an instance of OmexServiceContext</summary>
		/// <param name="partitionId">Partition Id</param>
		/// <param name="replicaOrInstanceId">Replica or InstanceId</param>
		protected OmexServiceContext(Guid partitionId, long replicaOrInstanceId)
		{
			PartitionId = partitionId;
			ReplicaOrInstanceId = replicaOrInstanceId;
		}


		/// <summary>Partition Id</summary>
		public Guid PartitionId { get; }


		/// <summary>Replica or Instance Id</summary>
		public long ReplicaOrInstanceId { get; }
	}
}
