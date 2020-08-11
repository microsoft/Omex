// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.Omex.Extensions.Abstractions.Activities;
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
		[DataRow(typeof(IAccessor<IStatelessServicePartition>), typeof(Accessor<IStatelessServicePartition>))]
		[DataRow(typeof(IAccessor<IServicePartition>), typeof(Accessor<IStatelessServicePartition>))]
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
		[DataRow(typeof(IAccessor<IStatefulServicePartition>), typeof(Accessor<IStatefulServicePartition>))]
		[DataRow(typeof(IAccessor<IServicePartition>), typeof(Accessor<IStatefulServicePartition>))]
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

			Assert.ThrowsException<AggregateException>(() => new HostBuilder()
				.ConfigureServices(c => c.AddTransient<TypeThatShouldNotBeResolvable>())
				.BuildStatelessService(serviceType, c => { }),
				"BuildStatelessService should fail in case of unresolvable dependencies");
		}

		[TestMethod]
		public void BuildStatefulService_LogsException()
		{
			string serviceType = "StatefulFailedServiceName";

			Assert.ThrowsException<AggregateException>(() => new HostBuilder()
				.ConfigureServices(c => c.AddTransient<TypeThatShouldNotBeResolvable>())
				.BuildStatefulService(serviceType, c => { }),
				"BuildStatefulService should fail in case of unresolvable dependencies");
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
	}
}
