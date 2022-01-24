// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions.UnitTests
{
	[TestClass]
	public class ServiceCollectionExtensionsTests
	{
		[TestMethod]
		public void Constructor_DI_InitializesPropertiesProperly()
		{
			// Arrange.
			ServiceFabricRestClientOptions settings = new() { ClusterEndpointFQDN = "moc" };
			IOptions<ServiceFabricRestClientOptions> options = Options.Create(settings);


			IServiceProvider provider = new ServiceCollection()
				.AddLogging()
				.AddSingleton(options)
				.AddServiceFabricClient()
				.AddServiceFabricClient()
				.BuildServiceProvider();

			// Act.
			IServiceFabricClientWrapper client = provider
				.GetRequiredService<IServiceFabricClientWrapper>();

			// Assert.
			Assert.IsNotNull(client);
			Assert.IsInstanceOfType(client, typeof(ServiceFabricClientWrapper));
		}
	}
}
