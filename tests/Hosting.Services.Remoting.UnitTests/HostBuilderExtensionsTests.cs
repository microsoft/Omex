// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Hosting.Services.UnitTests;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
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
			HostBuilder builder = new();
			ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatefulService, StatefulServiceContext>(builder);

			sfBuilder.AddRemotingListener<MockRemoteListenerBuilder<OmexStatefulService>>();

			IListenerBuilder<OmexStatefulService> value = builder.Build().Services.GetRequiredService<IListenerBuilder<OmexStatefulService>>();

			Assert.IsInstanceOfType(value, typeof(MockRemoteListenerBuilder<OmexStatefulService>));
		}

		[TestMethod]
		public void AddRemotingListener_ToStatelessService_RegisterType()
		{
			HostBuilder builder = new();
			ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatelessService, StatelessServiceContext>(builder);

			sfBuilder.AddRemotingListener<MockRemoteListenerBuilder<OmexStatelessService>>();

			IListenerBuilder<OmexStatelessService> value = builder.Build().Services.GetRequiredService<IListenerBuilder<OmexStatelessService>>();

			Assert.IsInstanceOfType(value, typeof(MockRemoteListenerBuilder<OmexStatelessService>));
		}

		[TestMethod]
		public void AddRemotingListener_UsingFunc_RegisterType()
		{
			string expectedName = nameof(AddRemotingListener_UsingFunc_RegisterType);
			FabricTransportRemotingListenerSettings settings = new();
			HostBuilder builder = new();
			ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatelessService, StatelessServiceContext>(builder);

			sfBuilder.AddRemotingListener(expectedName, (p, s) => new MockService(), settings);

			IListenerBuilder<OmexStatelessService> value = builder.Build().Services.GetRequiredService<IListenerBuilder<OmexStatelessService>>();

			Assert.IsNotNull(value);
			Assert.AreEqual(expectedName, value.Name);
			Assert.AreEqual(settings, GetSettings(value));
		}

		[TestMethod]
		public void AddRemotingListener_ToStatelessServiceUsingIServiceType_RegisterType()
		{
			string expectedName = nameof(AddRemotingListener_ToStatelessServiceUsingIServiceType_RegisterType);
			FabricTransportRemotingListenerSettings settings = new();
			HostBuilder builder = new();
			ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatelessService, StatelessServiceContext>(builder);

			sfBuilder.AddRemotingListener<MockService>(expectedName, settings);

			IListenerBuilder<OmexStatelessService> value = builder.Build().Services.GetRequiredService<IListenerBuilder<OmexStatelessService>>();

			Assert.IsInstanceOfType(value, typeof(GenericRemotingListenerBuilder<OmexStatelessService>));
			Assert.AreEqual(expectedName, value.Name);
			Assert.AreEqual(settings, GetSettings(value));
		}

		[TestMethod]
		public void AddRemotingListener_ToStatelessServiceUsingIServiceTypeThatHasUnregisteredDependency_BuildThrowsInvalidOperationException()
		{
			string expectedName = nameof(AddRemotingListener_ToStatelessServiceUsingIServiceType_RegisterType);
			FabricTransportRemotingListenerSettings settings = new();
			HostBuilder builder = new();
			ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatelessService, StatelessServiceContext>(builder);

			sfBuilder.AddRemotingListener<MockServiceWithDependencies>(expectedName, settings);

			IListenerBuilder<OmexStatelessService> value = builder.Build().Services.GetRequiredService<IListenerBuilder<OmexStatelessService>>();

			InvalidOperationException exception = Assert.ThrowsException<InvalidOperationException>(() => value.Build(MockServiceFabricServices.MockOmexStatelessService));
			StringAssert.Contains(exception.Message, typeof(MockServiceWithDependencies).FullName);
			StringAssert.Contains(exception.Message, typeof(IMockServiceDependency).FullName);
		}

		[TestMethod]
		public void AddRemotingListener_ToStatelessServiceUsingIServiceTypeThatHasRegisteredDependencies_BuildDoesNotThrow()
		{
			string expectedName = nameof(AddRemotingListener_ToStatelessServiceUsingIServiceType_RegisterType);
			FabricTransportRemotingListenerSettings settings = new();
			HostBuilder builder = new();
			ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatelessService, StatelessServiceContext>(builder);

			sfBuilder.ConfigureServices((_, services) => services.AddTransient<IMockServiceDependency, MockServiceDependency>());
			sfBuilder.AddRemotingListener<MockServiceWithDependencies>(expectedName, settings);

			IListenerBuilder<OmexStatelessService> value = builder.Build().Services.GetRequiredService<IListenerBuilder<OmexStatelessService>>();

			ICommunicationListener service = value.Build(MockServiceFabricServices.MockOmexStatelessService);

			Assert.IsNotNull(service);
			Assert.AreEqual(expectedName, value.Name);
			Assert.AreEqual(settings, GetSettings(value));
		}

		[TestMethod]
		public void AddRemotingListener_ToStatelessServiceUsingIServiceTypeThatHasNoDependencies_BuildDoesNotThrow()
		{
			string expectedName = nameof(AddRemotingListener_ToStatelessServiceUsingIServiceType_RegisterType);
			FabricTransportRemotingListenerSettings settings = new();
			HostBuilder builder = new();
			ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatelessService, StatelessServiceContext>(builder);

			sfBuilder.AddRemotingListener<MockService>(expectedName, settings);

			IListenerBuilder<OmexStatelessService> value = builder.Build().Services.GetRequiredService<IListenerBuilder<OmexStatelessService>>();

			ICommunicationListener service = value.Build(MockServiceFabricServices.MockOmexStatelessService);

			Assert.IsNotNull(service);
			Assert.AreEqual(expectedName, value.Name);
			Assert.AreEqual(settings, GetSettings(value));
		}

		[TestMethod]
		public void AddRemotingListener_ToStatefulServiceUsingIServiceType_RegisterType()
		{
			string expectedName = nameof(AddRemotingListener_ToStatefulServiceUsingIServiceType_RegisterType);
			FabricTransportRemotingListenerSettings settings = new();
			HostBuilder builder = new();
			ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext> sfBuilder =
				MockServiceFabricHostBuilder.CreateMockBuilder<OmexStatefulService, StatefulServiceContext>(builder);

			sfBuilder.AddRemotingListener<MockService>(expectedName, settings);

			IListenerBuilder<OmexStatefulService> value = builder.Build().Services.GetRequiredService<IListenerBuilder<OmexStatefulService>>();

			Assert.IsInstanceOfType(value, typeof(GenericRemotingListenerBuilder<OmexStatefulService>));
			Assert.AreEqual(expectedName, value.Name);
			Assert.AreEqual(settings, GetSettings(value));
		}

		private static FabricTransportRemotingListenerSettings? GetSettings<TService>(IListenerBuilder<TService> listenerBuilder)
			where TService : IServiceFabricService<ServiceContext>
				=> listenerBuilder is RemotingListenerBuilder<TService> remotingBuilder
					? remotingBuilder.Settings
					: null;
	}
}
