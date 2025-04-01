// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Registrator for OmexStatefulService.
	/// </summary>
	public sealed partial class OmexStatefulServiceRegistrator
	{
		/// <summary>
		/// Wrapper class for the ReplicaRole to be used as a parameter in generic types or methods.
		/// </summary>
		public class ReplicaRoleWrapper
		{
			/// <summary>
			/// Gets or sets the role of the replica.
			/// </summary>
			public ReplicaRole Role { get; set; }
		}
	}
}
