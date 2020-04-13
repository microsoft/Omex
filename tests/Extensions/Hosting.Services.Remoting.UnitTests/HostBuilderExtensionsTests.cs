// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Hosting.Services.UnitTests;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
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

			sfBuilder.AddRemotingListener<MockRemoteListenerBuilder<StatefulServiceContext>>();

			IListenerBuilder<StatefulServiceContext> value = builder.Build().Services.GetService<IListenerBuilder<StatefulServiceContext>>();

			Assert.IsInstanceOfType(value, typeof(MockRemoteListenerBuilder<StatefulServiceContext>));
		}

		[TestMethod]
		public void AddRemotingListener_ToStatelessService_RegisterType()
		{
			HostBuilder builder = new HostBuilder();
			ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatelessService, StatelessServiceContext>(builder);

			sfBuilder.AddRemotingListener<MockRemoteListenerBuilder<StatelessServiceContext>>();

			IListenerBuilder<StatelessServiceContext> value = builder.Build().Services.GetService<IListenerBuilder<StatelessServiceContext>>();

			Assert.IsInstanceOfType(value, typeof(MockRemoteListenerBuilder<StatelessServiceContext>));
		}

		[TestMethod]
		public void AddRemotingListener_UsingFunc_RegisterType()
		{
			string expectedName = nameof(AddRemotingListener_UsingFunc_RegisterType);
			FabricTransportRemotingListenerSettings settings = new FabricTransportRemotingListenerSettings();
			HostBuilder builder = new HostBuilder();
			ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatelessService, StatelessServiceContext>(builder);

			sfBuilder.AddRemotingListener(expectedName, (p, s) => new MockService(), settings);

			IListenerBuilder<StatelessServiceContext> value = builder.Build().Services.GetService<IListenerBuilder<StatelessServiceContext>>();

			Assert.IsNotNull(value);
			Assert.AreEqual(expectedName, value.Name);
			Assert.AreEqual(settings, GetSettings(value));
		}

		[TestMethod]
		public void AddRemotingListener_ToStatelessServiceUsingIServiceType_RegisterType()
		{
			string expectedName = nameof(AddRemotingListener_ToStatelessServiceUsingIServiceType_RegisterType);
			FabricTransportRemotingListenerSettings settings = new FabricTransportRemotingListenerSettings();
			HostBuilder builder = new HostBuilder();
			ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatelessService, StatelessServiceContext>(builder);

			sfBuilder.AddRemotingListener<MockService>(expectedName, settings);

			IListenerBuilder<StatelessServiceContext> value = builder.Build().Services.GetService<IListenerBuilder<StatelessServiceContext>>();

			Assert.IsInstanceOfType(value, typeof(GenericRemotingListenerBuilder<StatelessServiceContext>));
			Assert.AreEqual(expectedName, value.Name);
			Assert.AreEqual(settings, GetSettings(value));
		}

		[TestMethod]
		public void AddRemotingListener_ToStatefulServiceUsingIServiceType_RegisterType()
		{
			string expectedName = nameof(AddRemotingListener_ToStatefulServiceUsingIServiceType_RegisterType);
			FabricTransportRemotingListenerSettings settings = new FabricTransportRemotingListenerSettings();
			HostBuilder builder = new HostBuilder();
			ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatefulService, StatefulServiceContext>(builder);

			sfBuilder.AddRemotingListener<MockService>(expectedName, settings);

			IListenerBuilder<StatefulServiceContext> value = builder.Build().Services.GetService<IListenerBuilder<StatefulServiceContext>>();

			Assert.IsInstanceOfType(value, typeof(GenericRemotingListenerBuilder<StatefulServiceContext>));
			Assert.AreEqual(expectedName, value.Name);
			Assert.AreEqual(settings, GetSettings(value));
		}

		private static FabricTransportRemotingListenerSettings? GetSettings<TContext>(IListenerBuilder<TContext> listenerBuilder)
			where TContext : ServiceContext
				=> listenerBuilder is RemotingListenerBuilder<TContext> remotingBuilder
					? remotingBuilder.Settings
					: null;
	}
}
