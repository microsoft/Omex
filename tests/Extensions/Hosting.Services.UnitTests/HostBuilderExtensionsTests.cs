// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Fabric;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.EventSources;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.ServiceFabric.Data;
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

		[DataTestMethod]
		[DataRow(typeof(IServiceContext), typeof(OmexServiceFabricContext))]
		[DataRow(typeof(IExecutionContext), typeof(ServiceFabricExecutionContext))]
		[DataRow(typeof(ITimedScopeProvider), null)]
		[DataRow(typeof(ILogger<HostBuilderExtensionsTests>), null)]
		[DataRow(typeof(IHostedService), typeof(OmexHostedService))]
		[DataRow(typeof(IOmexServiceRegistrator), typeof(OmexStatelessServiceRegistrator))]
		[DataRow(typeof(IAccessor<StatelessServiceContext>), typeof(Accessor<StatelessServiceContext>))]
		[DataRow(typeof(IAccessor<ServiceContext>), typeof(Accessor<StatelessServiceContext>))]
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
		[DataRow(typeof(IOmexServiceRegistrator), typeof(OmexStatefulServiceRegistrator))]
		[DataRow(typeof(IAccessor<StatefulServiceContext>), typeof(Accessor<StatefulServiceContext>))]
		[DataRow(typeof(IAccessor<ServiceContext>), typeof(Accessor<StatefulServiceContext>))]
		[DataRow(typeof(IAccessor<IReliableStateManager>), typeof(Accessor<IReliableStateManager>))]
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
				&& e.EventId == (int)EventSourcesEventIds.GenericHostFailed);

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
				&& e.EventId == (int)EventSourcesEventIds.GenericHostFailed);

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
