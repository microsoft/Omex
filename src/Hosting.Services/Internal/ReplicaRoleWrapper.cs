// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal sealed partial class OmexStatefulServiceRegistrator
	{
		/// <summary>
		/// Wrapper class for the ReplicaRole to be used as a parameter in generic types or methods.
		/// </summary>
		public class ReplicaRoleWrapper
		{
			// 'ReplicaRole' must be a reference type in order to use it as parameter 'TValue' in the generic type or method. That's why we use this wrapper class
			public ReplicaRole Role { get; set; }
		}

	}
}
