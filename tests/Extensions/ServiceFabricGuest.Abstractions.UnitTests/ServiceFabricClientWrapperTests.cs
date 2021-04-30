// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions.UnitTests
{
	[TestClass]
	public class ServiceFabricClientWrapperTests
	{
		[TestMethod]
		public void Constructor_InitializesProperly()
		{
			// Arrange.
			ServiceFabricRestClientOptions settings = new() { ClusterEndpoint = "fabric://moc" };
			IOptions<ServiceFabricRestClientOptions> options = Options.Create(settings);

			// Act.
			IServiceFabricClientWrapper client = new ServiceFabricClientWrapper(options);

			// Assert.
			Assert.IsNotNull(client.GetAsync());
		}
	}
}
