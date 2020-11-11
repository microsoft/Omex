// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
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
			string applicationName = "SomeApplicationName";
			string serviceName = "SomeServiceName";
			string nodeName = "SomeNodeName";
			IPAddress nodeIPOrFQDN = IPAddress.Parse("192.0.0.1");

			Environment.SetEnvironmentVariable(ServiceFabricExecutionContext.ApplicationNameVariableName, applicationName);
			Environment.SetEnvironmentVariable(ServiceFabricExecutionContext.ServiceNameVariableName, serviceName);
			Environment.SetEnvironmentVariable(ServiceFabricExecutionContext.NodeNameVariableName, nodeName);
			Environment.SetEnvironmentVariable(ServiceFabricExecutionContext.NodeIPOrFQDNVariableName, nodeIPOrFQDN.ToString());

			IExecutionContext info = new ServiceFabricExecutionContext(new Mock<IHostEnvironment>().Object);

			Assert.AreEqual(applicationName, info.ApplicationName);
			Assert.AreEqual(serviceName, info.ServiceName);
			StringAssert.Contains(info.MachineId, nodeName);
			Assert.AreEqual(nodeIPOrFQDN, info.ClusterIpAddress);
		}
	}
}
