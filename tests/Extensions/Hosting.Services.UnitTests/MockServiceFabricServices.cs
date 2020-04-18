// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
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
					new Accessor<StatelessServiceContext>(),
					Enumerable.Empty<IListenerBuilder<OmexStatelessService>>(),
					Enumerable.Empty<IServiceAction<OmexStatelessService>>()),
				MockStatelessServiceContextFactory.Default);

		public static OmexStatefulService MockOmexStatefulService { get; } =
			new OmexStatefulService(
				new OmexStatefulServiceRunner(
					new Mock<IHostEnvironment>().Object,
					new Accessor<StatefulServiceContext>(),
					new Accessor<IReliableStateManager>(),
					Enumerable.Empty<IListenerBuilder<OmexStatefulService>>(),
					Enumerable.Empty<IServiceAction<OmexStatefulService>>()),
				MockStatefulServiceContextFactory.Default);
	}
}
