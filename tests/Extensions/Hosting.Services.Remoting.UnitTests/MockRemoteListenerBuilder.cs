using System.Fabric;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Microsoft.Omex.Extensions.Hosting.Services.Remoting.UnitTests
{
	internal class MockRemoteListenerBuilder<TService> : RemotingListenerBuilder<TService>
		where TService : IServiceFabricService<ServiceContext>
	{
		public MockRemoteListenerBuilder() : base("TestListener") { }

		public override IService BuildService(TService service) => new MockService();
	}
}
