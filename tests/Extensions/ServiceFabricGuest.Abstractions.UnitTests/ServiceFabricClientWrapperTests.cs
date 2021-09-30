﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
			IServiceFabricClientWrapper wrapper = new ServiceFabricClientWrapper(options);
			IServiceFabricClient client = await wrapper.GetAsync(default).ConfigureAwait(false);

			// Assert.
			Assert.IsNotNull(client);
		}
	}
}
