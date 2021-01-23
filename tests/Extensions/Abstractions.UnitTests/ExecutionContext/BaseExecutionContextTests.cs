// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
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

			Environment.SetEnvironmentVariable(BaseExecutionContext.ClusterNameVariableName, clusterName);
			Environment.SetEnvironmentVariable(BaseExecutionContext.RegionNameVariableName, regionName);
			Environment.SetEnvironmentVariable(BaseExecutionContext.SliceNameVariableName, sliceName);

			Mock<IHostEnvironment> enviromentMock = new Mock<IHostEnvironment>();
			enviromentMock.SetupGet(e => e.EnvironmentName).Returns(enviroment);

			IExecutionContext info = new BaseExecutionContext(enviromentMock.Object);

			Assert.AreEqual(clusterName, info.Cluster);
			Assert.AreEqual(regionName, info.RegionName);
			Assert.AreEqual(sliceName, info.DeploymentSlice);

			Assert.AreEqual(enviroment, info.EnvironmentName);
			Assert.AreEqual(isPrivate, info.IsPrivateDeployment);
		}
	}
}
