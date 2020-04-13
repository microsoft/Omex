// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Data;
using Moq;
using ServiceFabric.Mocks;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	public static class MockServiceFabricServices
	{
		public static OmexStatelessService MockOmexStatelessService { get; } =
			new OmexStatelessService(
				new OmexStatelessServiceRunner(
					new Mock<IHostEnvironment>().Object,
					new Accessor<OmexStatelessService>(),
					new Accessor<StatelessServiceContext>(),
					Enumerable.Empty<IListenerBuilder<StatelessServiceContext>>(),
					Enumerable.Empty<IServiceAction<StatelessServiceContext>>()),
				MockStatelessServiceContextFactory.Default);

		public static OmexStatefulService MockOmexStatefulService { get; } =
			new OmexStatefulService(
				new OmexStatefulServiceRunner(
					new Mock<IHostEnvironment>().Object,
					new Accessor<OmexStatefulService>(),
					new Accessor<IReliableStateManager>(),
					new Accessor<StatefulServiceContext>(),
					Enumerable.Empty<IListenerBuilder<StatefulServiceContext>>(),
					Enumerable.Empty<IServiceAction<StatefulServiceContext>>()),
				MockStatefulServiceContextFactory.Default);
	}
}
