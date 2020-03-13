// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal interface IOmexServiceRunner
	{
		Task RunServiceAsync(CancellationToken cancellationToken);
	}
}
