// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal sealed partial class OmexStatefulServiceRegistrator : OmexServiceRegistrator<OmexStatefulService, StatefulServiceContext>
	{
		public OmexStatefulServiceRegistrator(
			IOptions<ServiceRegistratorOptions> options,
			IAccessorSetter<StatefulServiceContext> contextAccessor,
			IAccessorSetter<IStatefulServicePartition> partitionAccessor,
			IAccessorSetter<IReliableStateManager> stateAccessor,
			IAccessorSetter<ReplicaRoleWrapper> roleAccessor,
			IEnumerable<IListenerBuilder<OmexStatefulService>> listenerBuilders,
			IEnumerable<IServiceAction<OmexStatefulService>> serviceActions)
				: base(options, contextAccessor, listenerBuilders, serviceActions)
		{
			PartitionAccessor = partitionAccessor;
			StateAccessor = stateAccessor;
			RoleAccessor = roleAccessor;
		}

		public override Task RegisterAsync(CancellationToken cancellationToken) =>
			ServiceRuntime.RegisterServiceAsync(Options.ServiceTypeName, context => new OmexStatefulService(this, context), cancellationToken: cancellationToken);

		public IAccessorSetter<IReliableStateManager> StateAccessor { get; }

		public IAccessorSetter<IStatefulServicePartition> PartitionAccessor { get; }

		public IAccessorSetter<ReplicaRoleWrapper> RoleAccessor { get; }

	}
}
