// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>IServiceContext without any information</summary>
	internal class EmptyServiceContext : IServiceContext
	{
		public Guid PartitionId => Guid.Empty;

		public long ReplicaOrInstanceId => 0;
	}
}
