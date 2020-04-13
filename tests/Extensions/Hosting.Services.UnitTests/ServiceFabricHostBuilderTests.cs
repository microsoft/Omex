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
using ServiceFabric.Mocks;

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
				.AddServiceAction<TestServiceAction<StatelessServiceContext>>();

			IServiceAction<StatelessServiceContext> resolvedAction = hostBuilder
				.Build()
				.Services.GetService<IServiceAction<StatelessServiceContext>>();

			Assert.IsNotNull(resolvedAction);
		}

		[TestMethod]
		public void AddServiceAction_UsingTypeForStateful_RegisterInstance()
		{
			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext>(hostBuilder)
				.AddServiceAction<TestServiceAction<StatefulServiceContext>>();

			IServiceAction<StatefulServiceContext> resolvedAction = hostBuilder
				.Build()
				.Services.GetService<IServiceAction<StatefulServiceContext>>();

			Assert.IsNotNull(resolvedAction);
		}

		[TestMethod]
		public void AddServiceAction_UsingObject_RegisterInstance()
		{
			IServiceAction<ServiceContext> serviceAction = new Mock<IServiceAction<ServiceContext>>().Object;

			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<IServiceFabricService<ServiceContext>, ServiceContext>(hostBuilder).AddServiceAction(p => serviceAction);
			IServiceAction<ServiceContext> resolvedAction = hostBuilder
				.Build()
				.Services.GetService<IServiceAction<ServiceContext>>();

			Assert.AreEqual(serviceAction, resolvedAction);
		}

		[TestMethod]
		public async Task AddServiceAction_UsingFunc_RegisterInstance()
		{
			bool actionCalled = false;
			Task action(IServiceProvider provider, CancellationToken t)
			{
				Assert.IsNotNull(provider);
				actionCalled = true;
				return Task.CompletedTask;
			}

			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<IServiceFabricService<ServiceContext>, ServiceContext>(hostBuilder).AddServiceAction(action);

			IServiceAction<ServiceContext> resolvedAction = hostBuilder
				.Build()
				.Services.GetService<IServiceAction<ServiceContext>>();

			await resolvedAction.RunAsync(CancellationToken.None).ConfigureAwait(false);

			Assert.IsTrue(actionCalled);
		}

		[TestMethod]
		public void AddServiceListener_UsingTypeForStateless_RegisterInstance()
		{
			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>(hostBuilder)
				.AddServiceListener<TestListenerBuilder<StatelessServiceContext>>();

			IListenerBuilder<StatelessServiceContext> resolvedAction = hostBuilder
				.Build()
				.Services.GetService<IListenerBuilder<StatelessServiceContext>>();

			Assert.IsNotNull(resolvedAction);
		}

		[TestMethod]
		public void AddServiceListener_UsingTypeForStateful_RegisterInstance()
		{
			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>(hostBuilder)
				.AddServiceListener<TestListenerBuilder<StatelessServiceContext>>();

			IListenerBuilder<StatelessServiceContext> resolvedAction = hostBuilder
				.Build()
				.Services.GetService<IListenerBuilder<StatelessServiceContext>>();

			Assert.IsNotNull(resolvedAction);
		}

		[TestMethod]
		public void AddServiceListener_UsingObject_RegisterInstance()
		{
			IListenerBuilder<ServiceContext> listenerBuilder = new Mock<IListenerBuilder<ServiceContext>>().Object;

			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<IServiceFabricService<ServiceContext>, ServiceContext>(hostBuilder)
				.AddServiceListener(p => listenerBuilder);

			IListenerBuilder<ServiceContext> resolvedAction = hostBuilder
				.Build()
				.Services.GetService<IListenerBuilder<ServiceContext>>();

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

			IListenerBuilder<ServiceContext> resolvedBuilder = hostBuilder
				.Build().Services
				.GetService<IListenerBuilder<ServiceContext>>();

			ICommunicationListener resultedListener = resolvedBuilder.Build(MockStatelessServiceContextFactory.Default);

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

		private class TestListenerBuilder<TContext> : IListenerBuilder<TContext>
			where TContext : ServiceContext
		{
			public string Name => "TestName";

			public ICommunicationListener Build(TContext service) => new Mock<ICommunicationListener>().Object;
		}

		private class TestServiceAction<TContext> : IServiceAction<TContext>
			where TContext : ServiceContext
		{
			public Task RunAsync(CancellationToken cancellationToken) => Task.CompletedTask;
		}
	}
}
