// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>IServiceContext without any information</summary>
	internal class EmptyServiceContext : IServiceContext
	{
		public Guid GetPartitionId() => Guid.Empty;

		public long GetReplicaOrInstanceId() => 0;
	}
}
