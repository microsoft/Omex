// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services.Remoting.UnitTests
{
	internal class MockRemoteListenerBuilder<TService> : RemotingListenerBuilder<TService>
		where TService : IServiceFabricService<ServiceContext>
	{
		public MockRemoteListenerBuilder() : base("TestListener", new FabricTransportRemotingListenerSettings()) { }

		public override IService BuildService(TService service) => new MockService();
	}
}
