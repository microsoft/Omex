// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.EventSources;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	[TestClass]
	public class HostBuilderExtensionsTests
	{
		[DataTestMethod]
		[DataRow(typeof(IServiceContext), typeof(OmexServiceFabricContext))]
		[DataRow(typeof(IExecutionContext), typeof(ServiceFabricExecutionContext))]
		[DataRow(typeof(ITimedScopeProvider), null)]
		[DataRow(typeof(ILogger<HostBuilderExtensionsTests>), null)]
		public void AddOmexServiceFabricDependencies_TypesRegistered(Type typeToResolver, Type? expectedImplementationType)
		{
			void CheckTypeRegistration<TContext>() where TContext : ServiceContext
			{
				object obj = new ServiceCollection()
					.AddOmexServiceFabricDependencies<TContext>()
					.AddSingleton(new Mock<IHostEnvironment>().Object)
					.BuildServiceProvider()
					.GetService(typeToResolver);

				Assert.IsNotNull(obj, "Failed to resolve for {0}", typeof(TContext));

				if (expectedImplementationType != null)
				{
					Assert.IsInstanceOfType(obj, expectedImplementationType, "Wrong implementation type for {0}", typeof(TContext));
				}
			}

			CheckTypeRegistration<StatelessServiceContext>();
			CheckTypeRegistration<StatefulServiceContext>();
		}

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

		[DataTestMethod]
		[DataRow(typeof(IServiceContext), typeof(OmexServiceFabricContext))]
		[DataRow(typeof(IExecutionContext), typeof(ServiceFabricExecutionContext))]
		[DataRow(typeof(ITimedScopeProvider), null)]
		[DataRow(typeof(ILogger<HostBuilderExtensionsTests>), null)]
		[DataRow(typeof(IHostedService), typeof(OmexHostedService))]
		[DataRow(typeof(IOmexServiceRunner), typeof(OmexStatelessServiceRunner))]
		[DataRow(typeof(IServiceContextAccessor<StatelessServiceContext>), typeof(ServiceContextAccessor<StatelessServiceContext>))]
		[DataRow(typeof(IServiceContextAccessor<ServiceContext>), typeof(ServiceContextAccessor<StatelessServiceContext>))]
		public void BuildStatelessService_RegisterTypes(Type type, Type? expectedImplementationType)
		{
			object obj = new HostBuilder()
				.BuildStatelessService("TestStatelessService", c => { })
				.Services
				.GetService(type);

			Assert.IsNotNull(obj);

			if (expectedImplementationType != null)
			{
				Assert.IsInstanceOfType(obj, expectedImplementationType);
			}
		}

		[DataTestMethod]
		[DataRow(typeof(IServiceContext), typeof(OmexServiceFabricContext))]
		[DataRow(typeof(IExecutionContext), typeof(ServiceFabricExecutionContext))]
		[DataRow(typeof(ITimedScopeProvider), null)]
		[DataRow(typeof(ILogger<HostBuilderExtensionsTests>), null)]
		[DataRow(typeof(IHostedService), typeof(OmexHostedService))]
		[DataRow(typeof(IOmexServiceRunner), typeof(OmexStatefulServiceRunner))]
		[DataRow(typeof(IServiceContextAccessor<StatefulServiceContext>), typeof(ServiceContextAccessor<StatefulServiceContext>))]
		[DataRow(typeof(IServiceContextAccessor<ServiceContext>), typeof(ServiceContextAccessor<StatefulServiceContext>))]
		public void BuildStatefulService_RegiesterTypes(Type type, Type? expectedImplementationType)
		{
			object obj = new HostBuilder()
				.BuildStatefulService("TestStatefulService", c => { })
				.Services
				.GetService(type);

			Assert.IsNotNull(obj);

			if (expectedImplementationType != null)
			{
				Assert.IsInstanceOfType(obj, expectedImplementationType);
			}
		}

		[TestMethod]
		public void BuildStatelessService_LogsExceptions()
		{
			string serviceType = "StatelessFailedServiceName";
			CustomEventListener listener = new CustomEventListener();
			listener.EnableEvents(ServiceInitializationEventSource.Instance, EventLevel.Error);

			Assert.ThrowsException<AggregateException>(() => new HostBuilder()
				.ConfigureServices(c => c.AddTransient<TypeThatShouldNotBeResolvable>())
				.BuildStatelessService(serviceType, c => { }),
				"BuildStatelessService should fail in case of unresolvable dependencies");

			bool hasErrorEvent = listener.EventsInformation.Any(e =>
				serviceType == GetPayloadValue<string>(e, ServiceTypePayloadName)
				&& e.EventId == (int)EventSourcesEventIds.GenericHostBuildFailed);

			Assert.IsTrue(hasErrorEvent, "BuildStatelessService error should be logged");
		}

		[TestMethod]
		public void BuildStatefulService_LogsException()
		{
			string serviceType = "StatefulFailedServiceName";
			CustomEventListener listener = new CustomEventListener();
			listener.EnableEvents(ServiceInitializationEventSource.Instance, EventLevel.Error);

			Assert.ThrowsException<AggregateException>(() => new HostBuilder()
				.ConfigureServices(c => c.AddTransient<TypeThatShouldNotBeResolvable>())
				.BuildStatefulService(serviceType, c => { }),
				"BuildStatefulService should fail in case of unresolvable dependencies");

			bool hasErrorEvent = listener.EventsInformation.Any(e =>
				serviceType == GetPayloadValue<string>(e, ServiceTypePayloadName)
				&& e.EventId == (int)EventSourcesEventIds.GenericHostBuildFailed);

			Assert.IsTrue(hasErrorEvent, "BuildStatefulService error should be logged");
		}

		[TestMethod]
		public void UseApplicationName_OverridesApplicationName()
		{
			string expectedName = "TestApplicationName";

			string actualName = Host.CreateDefaultBuilder()
				.UseApplicationName(expectedName)
				.Build().Services.GetService<IHostEnvironment>().ApplicationName;

			Assert.AreEqual(expectedName, actualName);
		}

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

		public class TypeThatShouldNotBeResolvable
		{
			public TypeThatShouldNotBeResolvable(TypeThatIsNotRegistered value) { }

			public class TypeThatIsNotRegistered { }
		}

		private class CustomEventListener : EventListener
		{
			public List<EventWrittenEventArgs> EventsInformation { get; } = new List<EventWrittenEventArgs>();

			protected override void OnEventSourceCreated(EventSource eventSource)
			{
				base.OnEventSourceCreated(eventSource);
			}

			protected override void OnEventWritten(EventWrittenEventArgs eventData) => EventsInformation.Add(eventData);
		}

		private static TPayloadType? GetPayloadValue<TPayloadType>(EventWrittenEventArgs info, string name)
			where TPayloadType : class
		{
			int index = info.PayloadNames?.IndexOf(name) ?? -1;
			return (TPayloadType?)(index < 0 ? null : info.Payload?[index]);
		}

		private const string ServiceTypePayloadName = "serviceType";
	}
}
