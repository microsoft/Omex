// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Hosting.Certificates;
using Microsoft.ServiceFabric.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions.UnitTests
{
	[TestClass]
	public class ServiceFabricClientWrapperTests
	{
		[TestMethod]
		public async Task ServiceFabricClientWrapper_GetAsync_ReturnsClient()
		{
			// Arrange.
			ServiceFabricRestClientOptions settings = new() { ClusterEndpointFQDN = "moc" };
			IOptions<ServiceFabricRestClientOptions> options = Options.Create(settings);

			// Act.
			IServiceFabricClientWrapper wrapper = new ServiceFabricClientWrapper(options, new Mock<ICertificateReader>().Object);
			IServiceFabricClient client = await wrapper.GetAsync(default).ConfigureAwait(false);

			// Assert.
			Assert.IsNotNull(client);
		}

		[TestMethod]
		public async Task ServiceFabricClientWrapper_GetAsyncForHttps_ThrowsWhenNoCertNameProvided()
		{
			// Arrange.
			ServiceFabricRestClientOptions settings = new() { ClusterEndpointFQDN = "moc", ClusterEndpointProtocol = Uri.UriSchemeHttps };
			IOptions<ServiceFabricRestClientOptions> options = Options.Create(settings);

			// Act.
			IServiceFabricClientWrapper wrapper = new ServiceFabricClientWrapper(options, new Mock<ICertificateReader>().Object);

			// Assert.
			await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => wrapper.GetAsync(default));
		}

		[TestMethod]
		public async Task ServiceFabricClientWrapper_GetAsyncForHttps_ThrowsWhenNoCertProvided()
		{
			// Arrange.
			ServiceFabricRestClientOptions settings = new() { ClusterEndpointFQDN = "moc", ClusterEndpointProtocol = Uri.UriSchemeHttps, ClusterCertCommonName = "ClusterCert" };
			IOptions<ServiceFabricRestClientOptions> options = Options.Create(settings);

			// Act.
			IServiceFabricClientWrapper wrapper = new ServiceFabricClientWrapper(options, new Mock<ICertificateReader>().Object);

			// Assert.
			await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => wrapper.GetAsync(default));
		}
	}
}
