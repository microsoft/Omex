using System;
using System.Fabric;
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
		[DataRow(typeof(ILogger), null)]
		public void TestOmexTypeRegistration(Type typeToResolver, Type? expectedImplementationType)
		{
			object obj = new ServiceCollection()
				.AddOmexServiceFabricDependencies()
				.BuildServiceProvider()
				.GetServices(typeToResolver);

			Assert.IsNotNull(obj);

			if (expectedImplementationType != null)
			{
				Assert.IsInstanceOfType(obj, expectedImplementationType);
			}
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
		[DataRow(typeof(ILogger), null)]
		[DataRow(typeof(IHostedService), typeof(OmexHostedService))]
		[DataRow(typeof(IOmexServiceRunner), typeof(OmexStatelessServiceRunner))]
		[DataRow(typeof(IServiceContextAccessor<StatelessServiceContext>), typeof(OmexStatelessServiceRunner))]
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
		[DataRow(typeof(ILogger), null)]
		[DataRow(typeof(IHostedService), typeof(OmexHostedService))]
		[DataRow(typeof(IOmexServiceRunner), typeof(OmexStatefulServiceRunner))]
		[DataRow(typeof(IServiceContextAccessor<StatefulServiceContext>), typeof(OmexStatefulServiceRunner))]
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
	}
}
