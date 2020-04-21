// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal interface IOmexServiceRegistrator
	{
		Task RegisterAsync(CancellationToken cancellationToken);
	}
}
