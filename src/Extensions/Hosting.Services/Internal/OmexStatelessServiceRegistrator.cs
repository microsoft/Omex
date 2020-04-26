﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal sealed class OmexStatelessServiceRegistrator : OmexServiceRegistrator<OmexStatelessService, StatelessServiceContext>
	{
		public OmexStatelessServiceRegistrator(
			IHostEnvironment environment,
			IAccessorSetter<StatelessServiceContext> contextAccessor,
			IAccessorSetter<IStatelessServicePartition> partitionAccessor,
			IEnumerable<IListenerBuilder<OmexStatelessService>> listenerBuilders,
			IEnumerable<IServiceAction<OmexStatelessService>> serviceActions)
				: base(environment, contextAccessor, listenerBuilders, serviceActions)
		{
			PartitionAccessor = partitionAccessor;
		}

		public override Task RegisterAsync(CancellationToken cancellationToken) =>
			ServiceRuntime.RegisterServiceAsync(ApplicationName, context => new OmexStatelessService(this, context), cancellationToken: cancellationToken);

		public IAccessorSetter<IStatelessServicePartition> PartitionAccessor { get; }
	}
}
