// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Compatability;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.Omex.Extensions.ServiceFabric;

namespace Microsoft.Omex.Extensions.Abstractions
{
	internal class OmexStatelessService : IHostedService
	{
		public Task StartAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	internal class OmexStatefullService : IHostedService
	{
		public Task StartAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
