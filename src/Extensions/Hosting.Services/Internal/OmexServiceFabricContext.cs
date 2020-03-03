// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.Omex.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Create wrapper from Service Fabric context to provide log information
	/// </summary>
	internal sealed class OmexServiceFabricContext : IServiceContext
	{
		public OmexServiceFabricContext(IServiceContextAccessor<ServiceContext> accessor)
		{
			PartitionId = Guid.Empty;
			ReplicaOrInstanceId = 0L;
			accessor.OnContextAvailable(UpdateState);
		}


		/// <inheritdoc/>
		public Guid PartitionId { get; private set; }


		/// <inheritdoc/>
		public long ReplicaOrInstanceId { get; private set; }


		private void UpdateState(ServiceContext context)
		{
			PartitionId = context.PartitionId;
			ReplicaOrInstanceId = context.ReplicaOrInstanceId;
		}
	}
}
