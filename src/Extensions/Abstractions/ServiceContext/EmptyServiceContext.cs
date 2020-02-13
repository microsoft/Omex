// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>IServiceContext without any information</summary>
	internal class EmptyServiceContext : OmexServiceContext
	{
		/// <summary>Creates an instance of NullServiceContext</summary>
		public EmptyServiceContext() : base(Guid.Empty, 0L)
		{
		}
	}
}
