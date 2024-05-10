// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// IServiceContext without any information
	/// </summary>
	public class EmptyServiceContext : IServiceContext
	{
		/// <summary>
		/// PartitionId
		/// </summary>
		public Guid PartitionId => Guid.Empty;

		/// <summary>
		/// ReplicaOrInstanceId
		/// </summary>
		public long ReplicaOrInstanceId => 0;
	}
}
