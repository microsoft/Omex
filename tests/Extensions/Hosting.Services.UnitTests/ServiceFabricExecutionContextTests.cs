// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hosting.Services.UnitTests
{
	[TestClass]
	public class ServiceFabricExecutionContextTests
	{
		[TestMethod]
		public void TestExecutionContextInitialization()
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
			Mock<IServiceContextAccessor<StatelessServiceContext>> contextMock = new Mock<IServiceContextAccessor<StatelessServiceContext>>();
			IExecutionContext info = new ServiceFabricExecutionContext(enviromentMock.Object, contextMock.Object);

			Assert.ReferenceEquals(appName, info.ServiceName);
			Assert.ReferenceEquals(envirometnName, info.EnvironmentName);
			Assert.ReferenceEquals(regionName, info.RegionName);
			Assert.ReferenceEquals(clusterName, info.Cluster);
		}
	}
}
