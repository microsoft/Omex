// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Abstractions.ServiceContext
{
	/// <summary>
	/// IServiceContext without any information
	/// </summary>
	public class EmptyServiceContext : IServiceContext
	{
		/// <summary>
		/// Service fabric PartitionId
		/// </summary>
		public Guid PartitionId => Guid.Empty;

		/// <summary>
		/// Service fabric
		/// </summary>
		public long ReplicaOrInstanceId => 0;
	}
}
