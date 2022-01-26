// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	[TestClass]
	public class BaseExecutionContextTests
	{
		[DataTestMethod]
		[DataRow("Development", true)]
		[DataRow("Prod", false)]
		public void Constructor_InitializesPropertiesProperly(string enviroment, bool isPrivate)
		{
			string clusterName = "SomeClusterName";
			string regionName = "SomeRegionName";
			string sliceName = "SomeSliceName";
			string applicationName = "SomeApplicationName";
			string serviceName = "SomeServiceName";
			string nodeName = "SomeNodeName";
			IPAddress nodeIPOrFQDN = IPAddress.Parse("192.0.0.1");

			Environment.SetEnvironmentVariable(BaseExecutionContext.ClusterNameVariableName, clusterName);
			Environment.SetEnvironmentVariable(BaseExecutionContext.RegionNameVariableName, regionName);
			Environment.SetEnvironmentVariable(BaseExecutionContext.SliceNameVariableName, sliceName);
			Environment.SetEnvironmentVariable(BaseExecutionContext.ApplicationNameVariableName, applicationName);
			Environment.SetEnvironmentVariable(BaseExecutionContext.ServiceNameVariableName, serviceName);
			Environment.SetEnvironmentVariable(BaseExecutionContext.NodeNameVariableName, nodeName);
			Environment.SetEnvironmentVariable(BaseExecutionContext.NodeIPOrFQDNVariableName, nodeIPOrFQDN.ToString());

			Mock<IHostEnvironment> enviromentMock = new Mock<IHostEnvironment>();
			enviromentMock.SetupGet(e => e.EnvironmentName).Returns(enviroment);

			IExecutionContext info = new BaseExecutionContext(enviromentMock.Object);

			Assert.AreEqual(clusterName, info.Cluster);
			Assert.AreEqual(regionName, info.RegionName);
			Assert.AreEqual(sliceName, info.DeploymentSlice);

			Assert.AreEqual(enviroment, info.EnvironmentName);
			Assert.AreEqual(isPrivate, info.IsPrivateDeployment);

			Assert.AreEqual(applicationName, info.ApplicationName);
			Assert.AreEqual(serviceName, info.ServiceName);
			Assert.AreEqual(nodeName, info.NodeName);
			StringAssert.Contains(info.MachineId, nodeName);
			Assert.AreEqual(nodeIPOrFQDN, info.ClusterIpAddress);
		}
	}
}
