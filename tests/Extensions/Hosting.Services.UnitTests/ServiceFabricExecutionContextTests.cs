// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	[TestClass]
	public class ServiceFabricExecutionContextTests
	{
		[TestMethod]
		public void Constructor_InitializesPropertiesProperly()
		{
			string appName = "SomeName";
			string envirometnName = "SomeEnviroment";
			string regionName = "SomeRegion";
			string clusterName = "SomeClusterName";

			Environment.SetEnvironmentVariable("REGION_NAME", regionName);
			Environment.SetEnvironmentVariable("CLUSTER_NAME", clusterName);

			Mock<IHostEnvironment> enviromentMock = new Mock<IHostEnvironment>();
			enviromentMock.SetupGet(e => e.ApplicationName).Returns(appName);
			enviromentMock.SetupGet(e => e.EnvironmentName).Returns(envirometnName);
			Accessor<StatelessServiceContext> accessor = new Accessor<StatelessServiceContext>();
			IAccessorSetter<StatelessServiceContext> setter = accessor;
			IExecutionContext info = new ServiceFabricExecutionContext(enviromentMock.Object, accessor);
			setter.SetValue(MockStatelessServiceContextFactory.Default);

			Assert.AreEqual(appName, info.ServiceName);
			Assert.AreEqual(envirometnName, info.EnvironmentName);
			Assert.AreEqual(regionName, info.RegionName);
			Assert.AreEqual(clusterName, info.Cluster);
		}
	}
}
