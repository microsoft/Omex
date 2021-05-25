// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.


using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions.UnitTests
{
	[TestClass]
	public class ServiceFabricRestClientOptionsTests
	{
		[TestMethod]
		public void ServiceFabricRestClientOptions_Default_FQDN_Invalid()
		{
			// Arrange.
			Environment.SetEnvironmentVariable(ServiceFabricRestClientOptions.RuntimeConnectionAddressEvnVariableName, "local@@host19081");
			ServiceFabricRestClientOptions options = new();

			// Act.
			string clusterEndpoint = options.ClusterEndpointFQDN;

			// Assert.
			Assert.AreEqual(ServiceFabricRestClientOptions.DefaultServiceFabricClusterFQDN, clusterEndpoint);
		}

		[TestMethod]
		public void ServiceFabricRestClientOptions_Default_FQDN_Valid_Single()
		{
			// Arrange.
			Environment.SetEnvironmentVariable(ServiceFabricRestClientOptions.RuntimeConnectionAddressEvnVariableName, "localhost:19081");
			ServiceFabricRestClientOptions options = new();

			// Act.
			string clusterEndpoint = options.ClusterEndpointFQDN;

			// Assert.
			Assert.AreEqual(ServiceFabricRestClientOptions.DefaultServiceFabricClusterFQDN, clusterEndpoint);
		}

		[TestMethod]
		public void ServiceFabricRestClientOptions_Default_FQDN_Valid_Multi()
		{
			// Arrange.
			Environment.SetEnvironmentVariable(ServiceFabricRestClientOptions.RuntimeConnectionAddressEvnVariableName, "local:host:19081");
			ServiceFabricRestClientOptions options = new();

			// Act.
			string clusterEndpoint = options.ClusterEndpointFQDN;

			// Assert.
			Assert.AreEqual("local:host", clusterEndpoint);
		}

		[TestMethod]
		public void ServiceFabricRestClientOptions_Default_Port()
		{
			// Arrange.
			Environment.SetEnvironmentVariable(ServiceFabricRestClientOptions.RuntimeConnectionAddressEvnVariableName, "localhost:19081");
			const int defaultPort = 19080;
			ServiceFabricRestClientOptions options = new()
			{
				ClusterEndpointPort = defaultPort
			};

			// Act.
			int port = options.ClusterEndpointPort;

			// Assert.
			Assert.AreEqual(port, defaultPort);
		}

		[TestMethod]
		public void ServiceFabricRestClientOptions_Override_FQDN()
		{
			// Arrange.
			Environment.SetEnvironmentVariable(ServiceFabricRestClientOptions.RuntimeConnectionAddressEvnVariableName, "localhost:19081");
			const string overrideFQDN = "myFQDN";
			ServiceFabricRestClientOptions options = new()
			{
				ClusterEndpointFQDN = overrideFQDN
			};

			// Act.
			string fqdn = options.ClusterEndpointFQDN;

			// Assert.
			Assert.AreEqual(fqdn, overrideFQDN);
		}

		[TestMethod]
		public void ServiceFabricRestClientOptions_EndPoint_Build()
		{
			// Arrange.
			Environment.SetEnvironmentVariable(ServiceFabricRestClientOptions.RuntimeConnectionAddressEvnVariableName, "localhost:19081");
			const string overrideFQDN = "myFQDN";
			const int defaultPort = 19080;

			ServiceFabricRestClientOptions options = new()
			{
				ClusterEndpointFQDN = overrideFQDN,
				ClusterEndpointPort = defaultPort
			};

			// Act.
			string endpoint = "http://myFQDN:19080";

			// Assert.
			Assert.AreEqual(endpoint, options.ClusterEndpoint());
		}

		[TestMethod]
		public void ServiceFabricRestClientOptions_EndPoint_Default_Build()
		{
			// Arrange.
			Environment.SetEnvironmentVariable(ServiceFabricRestClientOptions.RuntimeConnectionAddressEvnVariableName, "localhost:19081");

			ServiceFabricRestClientOptions options = new();

			// Act.
			string endpoint = "http://localhost:19080";

			// Assert.
			Assert.AreEqual(endpoint, options.ClusterEndpoint());
		}
	}
}
