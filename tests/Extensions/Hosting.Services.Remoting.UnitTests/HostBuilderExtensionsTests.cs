using System.Fabric;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Hosting.Services.UnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.Services.Remoting.UnitTests
{
	[TestClass]
	public class HostBuilderExtensionsTests
	{
		[TestMethod]
		public void AddRemotingListener_ToStatefulService_RegisterType()
		{
			HostBuilder builder = new HostBuilder();
			ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatefulService, StatefulServiceContext>(builder);

			sfBuilder.AddRemotingListener<MockRemoteListenerBuilder<OmexStatefulService>>();

			IListenerBuilder<OmexStatefulService> value = builder.Build().Services.GetService<IListenerBuilder<OmexStatefulService>>();

			Assert.IsInstanceOfType(value, typeof(MockRemoteListenerBuilder<OmexStatefulService>));
		}

		[TestMethod]
		public void AddRemotingListener_ToStatelessService_RegisterType()
		{
			HostBuilder builder = new HostBuilder();
			ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatelessService, StatelessServiceContext>(builder);

			sfBuilder.AddRemotingListener<MockRemoteListenerBuilder<OmexStatelessService>>();

			IListenerBuilder<OmexStatelessService> value = builder.Build().Services.GetService<IListenerBuilder<OmexStatelessService>>();

			Assert.IsInstanceOfType(value, typeof(MockRemoteListenerBuilder<OmexStatelessService>));
		}

		[TestMethod]
		public void AddRemotingListener_UsingFunc_RegisterType()
		{
			HostBuilder builder = new HostBuilder();
			ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatelessService, StatelessServiceContext>(builder);

			sfBuilder.AddRemotingListener("TestName", (p, s) => new MockService());

			IListenerBuilder<OmexStatelessService> value = builder.Build().Services.GetService<IListenerBuilder<OmexStatelessService>>();

			Assert.IsNotNull(value);
		}
	}
}
