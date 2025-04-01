// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.ServiceFabric.Data;
using ServiceFabric.Mocks;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	public static class MockServiceFabricServices
	{
		public static OmexStatelessService MockOmexStatelessService { get; } =
			new OmexStatelessService(
				new OmexStatelessServiceRegistrator(
					Options.Create(new ServiceRegistratorOptions()),
					new Accessor<StatelessServiceContext>(),
					new Accessor<IStatelessServicePartition>(),
					Enumerable.Empty<IListenerBuilder<OmexStatelessService>>(),
					Enumerable.Empty<IServiceAction<OmexStatelessService>>()),
				MockStatelessServiceContextFactory.Default);

		public static OmexStatefulService MockOmexStatefulService { get; } =
			new OmexStatefulService(
				new OmexStatefulServiceRegistrator(
					Options.Create(new ServiceRegistratorOptions()),
					new Accessor<StatefulServiceContext>(),
					new Accessor<IStatefulServicePartition>(),
					new Accessor<IReliableStateManager>(new MockReliableStateManager()),
					new Accessor<OmexStatefulServiceRegistrator.ReplicaRoleWrapper>(),
					Enumerable.Empty<IListenerBuilder<OmexStatefulService>>(),
					Enumerable.Empty<IServiceAction<OmexStatefulService>>()),
				MockStatefulServiceContextFactory.Default);
	}
}
