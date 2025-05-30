﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.ServiceFabric.Data;
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
			HostBuilder hostBuilder = new();
			new ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>(hostBuilder)
				.AddServiceAction<TestServiceAction<OmexStatelessService>>();

			IServiceAction<OmexStatelessService> resolvedAction = hostBuilder
				.Build()
				.Services.GetRequiredService<IServiceAction<OmexStatelessService>>();

			Assert.IsNotNull(resolvedAction);
		}

		[TestMethod]
		public void AddServiceAction_UsingTypeForStateful_RegisterInstance()
		{
			HostBuilder hostBuilder = new();
			new ServiceFabricHostBuilder<OmexStatefulService, StatefulServiceContext>(hostBuilder)
				.AddServiceAction<TestServiceAction<OmexStatefulService>>();

			IServiceAction<OmexStatefulService> resolvedAction = hostBuilder
				.Build()
				.Services.GetRequiredService<IServiceAction<OmexStatefulService>>();

			Assert.IsNotNull(resolvedAction);
		}

		[TestMethod]
		public void AddServiceAction_UsingObject_RegisterInstance()
		{
			IServiceAction<IServiceFabricService<ServiceContext>> serviceAction = new Mock<IServiceAction<IServiceFabricService<ServiceContext>>>().Object;

			HostBuilder hostBuilder = new();
			new ServiceFabricHostBuilder<IServiceFabricService<ServiceContext>, ServiceContext>(hostBuilder).AddServiceAction(p => serviceAction);
			IServiceAction<IServiceFabricService<ServiceContext>> resolvedAction = hostBuilder
				.Build()
				.Services.GetRequiredService<IServiceAction<IServiceFabricService<ServiceContext>>>();

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

			HostBuilder hostBuilder = new();
			new ServiceFabricHostBuilder<IServiceFabricService<ServiceContext>, ServiceContext>(hostBuilder).AddServiceAction(action);

			IServiceAction<IServiceFabricService<ServiceContext>> resolvedAction = hostBuilder
				.Build()
				.Services.GetRequiredService<IServiceAction<IServiceFabricService<ServiceContext>>>();

			IServiceFabricService<ServiceContext> mockService = new Mock<IServiceFabricService<ServiceContext>>().Object;
			await resolvedAction.RunAsync(mockService, CancellationToken.None).ConfigureAwait(false);

			Assert.IsTrue(actionCalled);
		}

		[TestMethod]
		public void AddServiceListener_UsingTypeForStateless_RegisterInstance()
		{
			HostBuilder hostBuilder = new();
			new ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>(hostBuilder)
				.AddServiceListener<TestListenerBuilder<OmexStatelessService>>();

			IListenerBuilder<OmexStatelessService> resolvedAction = hostBuilder
				.Build()
				.Services.GetRequiredService<IListenerBuilder<OmexStatelessService>>();

			Assert.IsNotNull(resolvedAction);
		}

		[TestMethod]
		public void AddServiceListener_UsingTypeForStateful_RegisterInstance()
		{
			HostBuilder hostBuilder = new();
			new ServiceFabricHostBuilder<OmexStatelessService, StatelessServiceContext>(hostBuilder)
				.AddServiceListener<TestListenerBuilder<OmexStatelessService>>();

			IListenerBuilder<OmexStatelessService> resolvedAction = hostBuilder
				.Build()
				.Services.GetRequiredService<IListenerBuilder<OmexStatelessService>>();

			Assert.IsNotNull(resolvedAction);
		}

		[TestMethod]
		public void AddServiceListener_UsingObject_RegisterInstance()
		{
			IListenerBuilder<IServiceFabricService<ServiceContext>> listenerBuilder = new Mock<IListenerBuilder<IServiceFabricService<ServiceContext>>>().Object;

			HostBuilder hostBuilder = new();
			new ServiceFabricHostBuilder<IServiceFabricService<ServiceContext>, ServiceContext>(hostBuilder)
				.AddServiceListener(p => listenerBuilder);

			IListenerBuilder<IServiceFabricService<ServiceContext>> resolvedAction = hostBuilder
				.Build()
				.Services.GetRequiredService<IListenerBuilder<IServiceFabricService<ServiceContext>>>();

			ReferenceEquals(listenerBuilder, resolvedAction);
		}

		[TestMethod]
		public void AddServiceListener_UsingFunc_RegisterInstance()
		{
			ICommunicationListener listener = new Mock<ICommunicationListener>().Object;
			ICommunicationListener listenerBuilder(IServiceProvider provider, object context) => listener;

			HostBuilder hostBuilder = new();
			new ServiceFabricHostBuilder<IServiceFabricService<ServiceContext>, ServiceContext>(hostBuilder)
				.AddServiceListener("testName", listenerBuilder);

			IListenerBuilder<IServiceFabricService<ServiceContext>> resolvedBuilder = hostBuilder
				.Build()
				.Services.GetRequiredService<IListenerBuilder<IServiceFabricService<ServiceContext>>>();

			IServiceFabricService<ServiceContext> mockService = new Mock<IServiceFabricService<ServiceContext>>().Object;
			ICommunicationListener resultedListener = resolvedBuilder.Build(mockService);

			ReferenceEquals(listener, resultedListener);
		}

		[TestMethod]
		public void ConfigureServices_PropagatesTypesRegistration()
		{
			HostBuilder hostBuilder = new();

			new ServiceFabricHostBuilder<IServiceFabricService<ServiceContext>, ServiceContext>(hostBuilder)
				.ConfigureServices((context, collection) =>
				{
					collection.AddTransient<TestTypeToResolve>();
				});

			TestTypeToResolve obj = hostBuilder.Build().Services.GetRequiredService<TestTypeToResolve>();

			Assert.IsNotNull(obj);
		}

		[TestMethod]
		public void OmexStatefulService_Constructor_SetsOmexStateManagerWithUnknownRole()
		{
			StatefulServiceContext serviceContext = MockStatefulServiceContextFactory.Default;

			Accessor<StatefulServiceContext> contextAccessor = new();
			Accessor<IReliableStateManager> stateAccessor = new();
			Accessor<OmexStateManager> stateManagerAccessor = new();
			Accessor<IStatefulServicePartition> statefulServiceAccessor = new();

			OmexStatefulServiceRegistrator serviceRegistrator = new OmexStatefulServiceRegistrator(
			Options.Create(new ServiceRegistratorOptions()),
			contextAccessor,
			statefulServiceAccessor,
			stateAccessor,
			stateManagerAccessor,
			Enumerable.Empty<IListenerBuilder<OmexStatefulService>>(),
			Enumerable.Empty<IServiceAction<OmexStatefulService>>());

			OmexStatefulService service = new OmexStatefulService(serviceRegistrator, serviceContext);

			Assert.IsNotNull(service);

			Assert.AreEqual(serviceContext, ((IAccessor<StatefulServiceContext>)contextAccessor).Value);
			Assert.IsNotNull(((IAccessor<IReliableStateManager>)stateAccessor).Value);
			Assert.IsNotNull(((IAccessor<OmexStateManager>)stateManagerAccessor).Value);

			OmexStateManager stateManager = ((IAccessor<OmexStateManager>)stateManagerAccessor).Value;
			Assert.IsNotNull(stateManager);
			Assert.IsFalse(stateManager.IsReadable);
			Assert.IsFalse(stateManager.IsWritable);

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
