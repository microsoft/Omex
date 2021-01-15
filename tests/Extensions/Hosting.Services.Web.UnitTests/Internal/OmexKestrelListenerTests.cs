// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hosting.Services.Web.UnitTests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests.Internal
{
	[TestClass]
	public class ApplicationBuilderExtensionsTests
	{
		[TestMethod]
		public void BuildStatelessWebService_UseUniqueServiceUrl_Failing()
		{
			Assert.ThrowsException<ArgumentException>(() =>
			{
				new HostBuilder().BuildStatelessWebService<MockStartup>(
					"someService1",
					new WebEndpointInfo[0],
					ServiceFabricIntegrationOptions.UseUniqueServiceUrl | ServiceFabricIntegrationOptions.UseReverseProxyIntegration);
			});
		}

		[TestMethod]
		public void BuildStatelessWebService_UseUniqueServiceUrl_Failing()
		{
			IHost host = new HostBuilder().BuildStatelessWebService<MockStartup>(
				"someService2",
				WebEndpointInfo.CreateHttp());



			IListenerBuilder<OmexStatelessService>[] builders = host.Services.GetRequiredService<IEnumerable<IListenerBuilder<OmexStatelessService>>>().ToArray();
		}
	}

	[TestClass]
	public class OmexKestrelListenerTests
	{
		[DataTestMethod]
		[DataRow("somehost1", 483, new[] { "https://[::]:20081", "https://[::]:80", "https://[::]:483" }, "https://somehost:483")]
		[DataRow("somehost2", 8082, new[] { "https://+:8081", "https://+:8082", "https://+:8083" }, "https://somehost:8082")]
		public async Task OpenAsync_ReturnsProperAddress(string host, int port, string[] addresses, string expected)
		{
			string actual = await CreateListener(host, port, addresses).OpenAsync(CancellationToken.None);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public async Task CloseAsyncAndAbort_NotFailing()
		{
			ICommunicationListener listener = CreateListener(addresses: "https://localhost:80");
			await listener.CloseAsync(CancellationToken.None);
			listener.Abort();
		}

		private ICommunicationListener CreateListener(string publishAddress = "localhost", int port = 80, params string[] addresses)
		{
			Mock<IServerAddressesFeature> addressesMock = new Mock<IServerAddressesFeature>();
			addressesMock.SetupGet(m => m.Addresses).Returns(addresses);

			Mock<IFeatureCollection> featureMock = new Mock<IFeatureCollection>();
			featureMock.Setup(m => m.Get<IServerAddressesFeature>()).Returns(addressesMock.Object);

			Mock<IServer> serverMock = new Mock<IServer>();
			serverMock.SetupGet(m => m.Features).Returns(featureMock.Object);

			return new OmexKestrelListener(serverMock.Object, publishAddress, port);
		}
	}

	[TestClass]
	public class WebEndpointInfoTests
	{
		[TestMethod]
		public void CreateHttp_SetsProperties()
		{
			string endpointName = "TestHttpEndpointName";
			int port = 8081;

			SetPortVariable(endpointName, port);

			WebEndpointInfo info = WebEndpointInfo.CreateHttp(endpointName);
			Assert.AreEqual(endpointName, info.Name);
			Assert.IsNull(info.SettingForCertificateCommonName);
			Assert.IsFalse(info.UseHttps);
			Assert.AreEqual(port, info.Port);
			Assert.AreEqual($"http://+:{port}", info.GetListenerUrl());
		}

		[TestMethod]
		public void CreateHttps_SetsProperties()
		{
			string endpointName = "TestHttpsEndpointName";
			string settingForCertificateCommonName = "SomeCertName";
			int port = 8083;

			SetPortVariable(endpointName, port);

			WebEndpointInfo info = WebEndpointInfo.CreateHttps(endpointName, settingForCertificateCommonName);
			Assert.AreEqual(endpointName, info.Name);
			Assert.AreEqual(settingForCertificateCommonName, info.SettingForCertificateCommonName);
			Assert.IsTrue(info.UseHttps);
			Assert.AreEqual(port, info.Port);
			Assert.AreEqual($"https://+:{port}", info.GetListenerUrl());
		}

		[TestMethod]
		public void CreateHttp_WithEmptyEndpointName_Throws() => Assert.ThrowsException<ArgumentException>(() => WebEndpointInfo.CreateHttp(string.Empty));

		[TestMethod]
		public void CreateHttps_WithEmptyEndpointName_Throws() => Assert.ThrowsException<ArgumentException>(() => WebEndpointInfo.CreateHttps(string.Empty));

		[TestMethod]
		public void CreateHttp_WithWrongEndpointName_Throws() => Assert.ThrowsException<ArgumentException>(() => WebEndpointInfo.CreateHttp("UnexistingEndpoint"));

		private void SetPortVariable(string name, int port) =>
			Environment.SetEnvironmentVariable(SfConfigurationProvider.EndpointPortEvnVariableSuffix + name, port.ToString(), EnvironmentVariableTarget.Process);
	}
}
