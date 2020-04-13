// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Microsoft.Omex.Extensions.Hosting.Services.Remoting.UnitTests
{
	internal class MockRemoteListenerBuilder<TContext> : RemotingListenerBuilder<TContext>
		where TContext : ServiceContext
	{
		public MockRemoteListenerBuilder() : base("TestListener") { }

		public override IService BuildService(TContext service) => new MockService();
	}
}
