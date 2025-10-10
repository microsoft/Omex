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

			// Act.
			ServiceFabricRestClientOptions options = new();
			string clusterEndpointFQDN = options.ClusterEndpointFQDN;

			// Assert.
			Assert.AreEqual(ServiceFabricRestClientOptions.DefaultServiceFabricClusterFQDN, clusterEndpointFQDN);
		}

		[TestMethod]
		public void ServiceFabricRestClientOptions_Default_FQDN_Valid_Single()
		{
			// Arrange.
			Environment.SetEnvironmentVariable(ServiceFabricRestClientOptions.RuntimeConnectionAddressEvnVariableName, "localhost:19081");

			// Act.
			ServiceFabricRestClientOptions options = new();
			string clusterEndpointFQDN = options.ClusterEndpointFQDN;

			// Assert.
			Assert.AreEqual(ServiceFabricRestClientOptions.DefaultServiceFabricClusterFQDN, clusterEndpointFQDN);
		}

		[TestMethod]
		public void ServiceFabricRestClientOptions_Default_FQDN_Valid_Multi()
		{
			// Arrange.
			Environment.SetEnvironmentVariable(ServiceFabricRestClientOptions.RuntimeConnectionAddressEvnVariableName, "local:host:19081");

			// Act.
			ServiceFabricRestClientOptions options = new();
			string clusterEndpointFQDN = options.ClusterEndpointFQDN;

			// Assert.
			Assert.AreEqual("local:host", clusterEndpointFQDN);
		}

		[TestMethod]
		public void ServiceFabricRestClientOptions_Default_Port()
		{
			// Arrange.
			Environment.SetEnvironmentVariable(ServiceFabricRestClientOptions.RuntimeConnectionAddressEvnVariableName, "localhost:19081");
			const int defaultPort = 19080;

			// Act.
			ServiceFabricRestClientOptions options = new()
			{
				ClusterEndpointPort = defaultPort
			};

			int port = options.ClusterEndpointPort;

			// Assert.
			Assert.AreEqual(defaultPort, port);
		}

		[TestMethod]
		public void ServiceFabricRestClientOptions_Override_FQDN()
		{
			// Arrange.
			Environment.SetEnvironmentVariable(ServiceFabricRestClientOptions.RuntimeConnectionAddressEvnVariableName, "localhost:19081");
			const string overrideFQDN = "myFQDN";

			// Act.
			ServiceFabricRestClientOptions options = new()
			{
				ClusterEndpointFQDN = overrideFQDN
			};
			string fqdn = options.ClusterEndpointFQDN;

			// Assert.
			Assert.AreEqual(overrideFQDN, fqdn);
		}

		[TestMethod]
		public void ServiceFabricRestClientOptions_EndPoint_Build()
		{
			// Arrange.
			Environment.SetEnvironmentVariable(ServiceFabricRestClientOptions.RuntimeConnectionAddressEvnVariableName, "localhost:19081");
			const string overrideFQDN = "myFQDN";
			const int defaultPort = 19080;

			// Act.
			ServiceFabricRestClientOptions options = new()
			{
				ClusterEndpointFQDN = overrideFQDN,
				ClusterEndpointPort = defaultPort,
				ClusterEndpointProtocol = Uri.UriSchemeHttps
			};
			Uri endpoint = new Uri("https://myFQDN:19080/");

			// Assert.
			Assert.AreEqual(endpoint, options.GetClusterEndpoint());
		}

		[TestMethod]
		public void ServiceFabricRestClientOptions_EndPoint_Default_Build()
		{
			// Arrange.
			Environment.SetEnvironmentVariable(ServiceFabricRestClientOptions.RuntimeConnectionAddressEvnVariableName, "localhost:19081");

			// Act.
			ServiceFabricRestClientOptions options = new()
			{
				ClusterEndpointProtocol = Uri.UriSchemeHttps
			};

			Uri endpoint = new Uri("https://localhost:19080/");

			// Assert.
			Assert.AreEqual(endpoint, options.GetClusterEndpoint());
		}
	}
}
