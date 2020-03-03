// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Interface with service context inforormation
	/// </summary>
	public interface IServiceContext
	{
		/// <summary>
		/// Partition Id
		/// </summary>
		Guid PartitionId { get; }


		/// <summary>
		/// Replica Id or Instance Id
		/// </summary>
		long ReplicaOrInstanceId { get; }
	}
}
