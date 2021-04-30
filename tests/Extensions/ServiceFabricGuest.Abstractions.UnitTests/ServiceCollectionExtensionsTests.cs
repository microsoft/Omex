// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Client;
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
			ServiceFabricRestClientOptions settings = new() { ClusterEndpoint = "fabric://moc" };
			IOptions<ServiceFabricRestClientOptions> options = Options.Create(settings);


			IServiceProvider provider = new ServiceCollection()
				.AddSingleton(options)
				.AddServiceFabricClient()
				.AddServiceFabricClient()
				.BuildServiceProvider();

			// Act.
			IServiceFabricClientWrapper[] clients = provider
				.GetRequiredService<IEnumerable<IServiceFabricClientWrapper>>()
				.ToArray();

			// Assert.
			Assert.AreEqual(1, clients.Length, "Published should be registered once");
			Assert.IsInstanceOfType(clients[0], typeof(ServiceFabricClientWrapper));
		}

		[TestMethod]
		public async Task GetAsync_GetterWorksProperlyAsync()
		{
			// Arrange.
			ServiceFabricRestClientOptions settings = new() { ClusterEndpoint = "http://moc" };
			IOptions<ServiceFabricRestClientOptions> options = Options.Create(settings);
			IServiceFabricClientWrapper wrapper = new ServiceFabricClientWrapper(options);

			// Act
			IServiceFabricClient? client = await wrapper.GetAsync();

			// Assert
			Assert.IsNotNull(client);
		}
	}
}
