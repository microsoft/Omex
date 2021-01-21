// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests.Internal
{
	[TestClass]
	public class OmexKestrelListenerTests
	{
		[DataTestMethod]
		[DataRow("somehost1", 483, new[] { "https://[::]:20081", "https://[::]:80", "https://[::]:483" }, "https://somehost1:483")]
		[DataRow("somehost2", 8082, new[] { "https://+:8081", "https://+:8082", "https://+:8083" }, "https://somehost2:8082")]
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
}
