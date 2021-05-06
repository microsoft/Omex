// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions.UnitTests
{
	[TestClass]
	public class FactoryTests
	{
		[TestMethod]
		public void Factory_ServiceFabricClientWrapperCreatedProperly()
		{
			// Arrange.
			ServiceFabricRestClientOptions settings = new() { ClusterEndpoint = "fabric://moc" };
			IOptions<ServiceFabricRestClientOptions> options = Options.Create(settings);

			// Act.
			IServiceFabricClientWrapper client = ServiceFabricClientFactory.CreateServiceFabricClientWrapper(options);

			// Assert.
			Assert.IsNotNull(client);
			Assert.IsInstanceOfType(client, typeof(ServiceFabricClientWrapper));
		}
	}
}
