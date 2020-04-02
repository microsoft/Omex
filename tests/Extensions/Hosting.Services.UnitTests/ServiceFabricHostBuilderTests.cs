// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	[TestClass]
	public class ServiceFabricHostBuilderTests
	{
		[TestMethod]
		public void AddServiceAction_UsingTypeForStateless_RegisterInstance()
		{
			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>(hostBuilder)
				.AddServiceAction<TestServiceAction<OmexStatelessService>>();

			IServiceAction<OmexStatelessService> resolvedAction = hostBuilder
				.Build()
				.Services.GetService<IServiceAction<OmexStatelessService>>();

			Assert.IsNotNull(resolvedAction);
		}

		[TestMethod]
		public void AddServiceAction_UsingTypeForStateful_RegisterInstance()
		{
			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext>(hostBuilder)
				.AddServiceAction<TestServiceAction<OmexStatefulService>>();

			IServiceAction<OmexStatefulService> resolvedAction = hostBuilder
				.Build()
				.Services.GetService<IServiceAction<OmexStatefulService>>();

			Assert.IsNotNull(resolvedAction);
		}

		[TestMethod]
		public void AddServiceAction_UsingObject_RegisterInstance()
		{
			IServiceAction<IServiceFabricService<ServiceContext>> serviceAction = new Mock<IServiceAction<IServiceFabricService<ServiceContext>>>().Object;

			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<IServiceFabricService<ServiceContext>, ServiceContext>(hostBuilder).AddServiceAction(p => serviceAction);
			IServiceAction<IServiceFabricService<ServiceContext>> resolvedAction = hostBuilder
				.Build()
				.Services.GetService<IServiceAction<IServiceFabricService<ServiceContext>>>();

			Assert.AreEqual(serviceAction, resolvedAction);
		}

		[TestMethod]
		public async Task AddServiceAction_UsingFunc_RegisterInstance()
		{
			bool actionCalled = false;
			Task action(IServiceProvider provider, object c, CancellationToken t)
			{
				Assert.IsNotNull(provider);
				actionCalled = true;
				return Task.CompletedTask;
			}

			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<IServiceFabricService<ServiceContext>, ServiceContext>(hostBuilder).AddServiceAction(action);

			IServiceAction<IServiceFabricService<ServiceContext>> resolvedAction = hostBuilder
				.Build()
				.Services.GetService<IServiceAction<IServiceFabricService<ServiceContext>>>();

			IServiceFabricService<ServiceContext> mockService = new Mock<IServiceFabricService<ServiceContext>>().Object;
			await resolvedAction.RunAsync(mockService, CancellationToken.None).ConfigureAwait(false);

			Assert.IsTrue(actionCalled);
		}

		[TestMethod]
		public void AddServiceListener_UsingTypeForStateless_RegisterInstance()
		{
			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>(hostBuilder)
				.AddServiceListener<TestListenerBuilder<OmexStatelessService>>();

			IListenerBuilder<OmexStatelessService> resolvedAction = hostBuilder
				.Build()
				.Services.GetService<IListenerBuilder<OmexStatelessService>>();

			Assert.IsNotNull(resolvedAction);
		}

		[TestMethod]
		public void AddServiceListener_UsingTypeForStateful_RegisterInstance()
		{
			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>(hostBuilder)
				.AddServiceListener<TestListenerBuilder<OmexStatelessService>>();

			IListenerBuilder<OmexStatelessService> resolvedAction = hostBuilder
				.Build()
				.Services.GetService<IListenerBuilder<OmexStatelessService>>();

			Assert.IsNotNull(resolvedAction);
		}

		[TestMethod]
		public void AddServiceListener_UsingObject_RegisterInstance()
		{
			IListenerBuilder<IServiceFabricService<ServiceContext>> listenerBuilder = new Mock<IListenerBuilder<IServiceFabricService<ServiceContext>>>().Object;

			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<IServiceFabricService<ServiceContext>, ServiceContext>(hostBuilder)
				.AddServiceListener(p => listenerBuilder);

			IListenerBuilder<IServiceFabricService<ServiceContext>> resolvedAction = hostBuilder
				.Build()
				.Services.GetService<IListenerBuilder<IServiceFabricService<ServiceContext>>>();

			ReferenceEquals(listenerBuilder, resolvedAction);
		}

		[TestMethod]
		public void AddServiceListener_UsingFunc_RegisterInstance()
		{
			ICommunicationListener listener = new Mock<ICommunicationListener>().Object;
			ICommunicationListener listenerBuilder(IServiceProvider provider, object context) => listener;

			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<IServiceFabricService<ServiceContext>, ServiceContext>(hostBuilder)
				.AddServiceListener("testName", listenerBuilder);

			IListenerBuilder<IServiceFabricService<ServiceContext>> resolvedBuilder = hostBuilder
				.Build().Services
				.GetService<IListenerBuilder<IServiceFabricService<ServiceContext>>>();

			IServiceFabricService<ServiceContext> mockService = new Mock<IServiceFabricService<ServiceContext>>().Object;
			ICommunicationListener resultedListener = resolvedBuilder.Build(mockService);

			ReferenceEquals(listener, resultedListener);
		}

		[TestMethod]
		public void ConfigureServices_PropagatesTypesRegistration()
		{
			HostBuilder hostBuilder = new HostBuilder();

			new ServiceFabricHostBuilder<IServiceFabricService<ServiceContext>, ServiceContext>(hostBuilder)
				.ConfigureServices((context, collection) =>
				{
					collection.AddTransient<TestTypeToResolve>();
				});

			TestTypeToResolve obj = hostBuilder.Build().Services.GetService<TestTypeToResolve>();

			Assert.IsNotNull(obj);
		}

		private class TestTypeToResolve { }

		private class TestListenerBuilder<TService> : IListenerBuilder<TService>
			where TService : IServiceFabricService<ServiceContext>
		{
			public string Name => "TestName";

			public ICommunicationListener Build(TService service) => new Mock<ICommunicationListener>().Object;
		}

		private class TestServiceAction<TService> : IServiceAction<TService>
			where TService : IServiceFabricService<ServiceContext>
		{
			public Task RunAsync(TService service, CancellationToken cancellationToken) => Task.CompletedTask;
		}
	}
}
