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
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.Omex.Extensions.TimedScopes;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;

namespace Hosting.Services.UnitTests
{
	[TestClass]
	public class HostBuilderExtensionsTests
	{
		[DataTestMethod]
		[DataRow(typeof(IServiceContext), typeof(OmexServiceFabricContext))]
		[DataRow(typeof(IMachineInformation), typeof(ServiceFabricMachineInformation))]
		[DataRow(typeof(ITimedScopeProvider), null)]
		[DataRow(typeof(ILogger<HostBuilderExtensionsTests>), null)]
		public void TestOmexTypeRegistration(Type typeToResolver, Type? expectedImplementationType)
		{
			void CheckTypeRegistration<TContext>() where TContext : ServiceContext
			{
				object obj = new ServiceCollection()
					.AddOmexServiceFabricDependencies<StatelessServiceContext>()
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
		public void CheckServiceActionRegistration()
		{
			IServiceAction<ServiceContext> serviceAction = new Mock<IServiceAction<ServiceContext>>().Object;

			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<ServiceContext>(hostBuilder).AddServiceAction(serviceAction);
			IServiceAction<ServiceContext> resolvedAction = hostBuilder.Build().Services.GetService<IServiceAction<ServiceContext>>();

			Assert.ReferenceEquals(serviceAction, resolvedAction);
		}


		[TestMethod]
		public async Task CheckServiceActionRegistrationUsingFunc()
		{
			bool actionCalled = false;
			Func<ServiceContext, CancellationToken, Task> action = (c, t) =>
			{
				actionCalled = true;
				return Task.CompletedTask;
			};

			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<ServiceContext>(hostBuilder).AddServiceAction(action);

			IServiceAction<ServiceContext> resolvedAction = hostBuilder.Build().Services.GetService<IServiceAction<ServiceContext>>();

			await resolvedAction.RunAsync(MockStatelessServiceContextFactory.Default, CancellationToken.None);

			Assert.IsTrue(actionCalled);
		}


		[TestMethod]
		public void CheckServiceListenerRegistration()
		{
			IListenerBuilder<ServiceContext> listenerBuilder = new Mock<IListenerBuilder<ServiceContext>>().Object;

			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<ServiceContext>(hostBuilder).AddServiceListener(listenerBuilder);
			IListenerBuilder<ServiceContext> resolvedAction = hostBuilder.Build().Services.GetService<IListenerBuilder<ServiceContext>>();

			Assert.ReferenceEquals(listenerBuilder, resolvedAction);
		}


		[TestMethod]
		public void CheckServiceListenerRegistrationUsingFunc()
		{
			ICommunicationListener listener = new Mock<ICommunicationListener>().Object;
			Func<ServiceContext, ICommunicationListener> listenerBuilder = context => listener;

			HostBuilder hostBuilder = new HostBuilder();
			new ServiceFabricHostBuilder<ServiceContext>(hostBuilder).AddServiceListener("testName", listenerBuilder);
			IListenerBuilder<ServiceContext> resolvedBuilder = hostBuilder.Build().Services.GetService<IListenerBuilder<ServiceContext>>();
			ICommunicationListener resultedListener = resolvedBuilder.Build(MockStatelessServiceContextFactory.Default);

			Assert.ReferenceEquals(listener, resultedListener);
		}


		[DataTestMethod]
		[DataRow(typeof(IServiceContext), typeof(OmexServiceFabricContext))]
		[DataRow(typeof(IMachineInformation), typeof(ServiceFabricMachineInformation))]
		[DataRow(typeof(ITimedScopeProvider), null)]
		[DataRow(typeof(ILogger<HostBuilderExtensionsTests>), null)]
		[DataRow(typeof(IHostedService), typeof(OmexHostedService))]
		[DataRow(typeof(IOmexServiceRunner), typeof(OmexStatelessServiceRunner))]
		[DataRow(typeof(IServiceContextAccessor<StatelessServiceContext>), typeof(ServiceContextAccessor<StatelessServiceContext>))]
		[DataRow(typeof(IServiceContextAccessor<ServiceContext>), typeof(ServiceContextAccessor<StatelessServiceContext>))]
		public void CheckBuildeStatelessService(Type type, Type? expectedImplementationType)
		{
			object obj = new HostBuilder()
				.BuildStatelessService(c => { })
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
		[DataRow(typeof(IMachineInformation), typeof(ServiceFabricMachineInformation))]
		[DataRow(typeof(ITimedScopeProvider), null)]
		[DataRow(typeof(ILogger<HostBuilderExtensionsTests>), null)]
		[DataRow(typeof(IHostedService), typeof(OmexHostedService))]
		[DataRow(typeof(IOmexServiceRunner), typeof(OmexStatefulServiceRunner))]
		[DataRow(typeof(IServiceContextAccessor<StatefulServiceContext>), typeof(ServiceContextAccessor<StatefulServiceContext>))]
		[DataRow(typeof(IServiceContextAccessor<ServiceContext>), typeof(ServiceContextAccessor<StatefulServiceContext>))]
		public void CheckBuildeStatefulService(Type type, Type? expectedImplementationType)
		{
			object obj = new HostBuilder()
				.BuildStatefullService(c => { })
				.Services
				.GetService(type);

			Assert.IsNotNull(obj);

			if (expectedImplementationType != null)
			{
				Assert.IsInstanceOfType(obj, expectedImplementationType);
			}
		}


		[TestMethod]
		public void TestExceptionHandlingAndReporting()
		{
			CustomEventListener listener = new CustomEventListener();
			listener.EnableEvents(ServiceInitializationEventSource.Instance, EventLevel.Error);

			Assert.ThrowsException<AggregateException>(() =>  new HostBuilder()
				.ConfigureServices(c => c.AddTransient<TypeThatShouldNotBeResolvable>())
				.BuildStatelessService(c => { }),
				"BuildStatelessService should fail in case of unresolvable dependencies");


			Assert.AreEqual(1, listener.EventsInformation.Count(), "BuildStatelessService error should be logged");
			listener.EventsInformation.Clear();

			Assert.ThrowsException<AggregateException>(() => new HostBuilder()
				.ConfigureServices(c => c.AddTransient<TypeThatShouldNotBeResolvable>())
				.BuildStatefullService(c => { }),
				"BuildStatefullService should fail in case of unresolvable dependencies");

			Assert.AreEqual(1, listener.EventsInformation.Count(), "BuildStatefullService error should be logged");
			listener.EventsInformation.Clear();
		}


		public class TypeThatShouldNotBeResolvable
		{
			public TypeThatShouldNotBeResolvable(TypeThatIsNotRegistred value) { }

			public class TypeThatIsNotRegistred { }
		}


		private class CustomEventListener : EventListener
		{
			public List<EventWrittenEventArgs> EventsInformation { get; } = new List<EventWrittenEventArgs>();

			protected override void OnEventWritten(EventWrittenEventArgs eventData) => EventsInformation.Add(eventData);
		}
	}
}
