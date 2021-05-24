// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions.UnitTests
{
	[TestClass]
	public class ServiceFabricClientFactoryTests
	{
		[TestMethod]
		public void ServiceFabricClientFactory_ServiceFabricClientWrapperCreatedProperly()
		{
			// Arrange.
			ServiceFabricRestClientOptions settings = new() { ClusterEndpointFQDN = "fabric://moc" };

			// Act.
			IServiceFabricClientWrapper client = ServiceFabricClientFactory.Create(settings);

			// Assert.
			Assert.IsNotNull(client);
			Assert.IsInstanceOfType(client, typeof(ServiceFabricClientWrapper));
		}
	}
}
