﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal sealed class OmexStatefulServiceRegistrator : OmexServiceRegistrator<OmexStatefulService, StatefulServiceContext>
	{
		public OmexStatefulServiceRegistrator(
			IHostEnvironment environment,
			IAccessorSetter<StatefulServiceContext> contextAccessor,
			IAccessorSetter<IStatefulServicePartition> partitionAccessor,
			IAccessorSetter<IReliableStateManager> stateAccessor,
			IEnumerable<IListenerBuilder<OmexStatefulService>> listenerBuilders,
			IEnumerable<IServiceAction<OmexStatefulService>> serviceActions)
				: base(environment, contextAccessor, listenerBuilders, serviceActions)
		{
			PartitionAccessor = partitionAccessor;
			StateAccessor = stateAccessor;
		}

		public override Task RegisterAsync(CancellationToken cancellationToken) =>
			ServiceRuntime.RegisterServiceAsync(ApplicationName, context => new OmexStatefulService(this, context), cancellationToken: cancellationToken);

		public IAccessorSetter<IReliableStateManager> StateAccessor { get; }

		public IAccessorSetter<IStatefulServicePartition> PartitionAccessor { get; }
	}
}
