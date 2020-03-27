// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	[TestClass]
	public class ServiceFabricHostBuilderTests
	{
		[TestMethod]
		public void ConfigureServices_PropagatesTypesRegistration()
		{
			HostBuilder hostBuilder = new HostBuilder();

			new ServiceFabricHostBuilder<IServiceFabricService<ServiceContext>, ServiceContext>(hostBuilder)
				.ConfigureServices((context, collection) =>
				{
					collection.AddTransient<TestTypeToResolve>();
				});

			TestTypeToResolve obj = hostBuilder.Build().Services.GetService<TestTypeToResolve>();

			Assert.IsNotNull(obj);
		}

		private class TestTypeToResolve { }
	}
}
