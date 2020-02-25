using System.Fabric;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hosting.Services.UnitTests
{
	[TestClass]
	public class ServiceFabricHostBuilderTests
	{
		[TestMethod]
		public void HostPropagatesTypesRegistration()
		{
			HostBuilder hostBuilder = new HostBuilder();

			new ServiceFabricHostBuilder<ServiceContext>(hostBuilder)
			.ConfigureServices((context, collection) =>
			{
				collection.AddTransient<TestTypeToResolve>();
			});

			TestTypeToResolve obj = hostBuilder.Build().Services.GetService<TestTypeToResolve>();

			Assert.IsNotNull(obj);
		}


		private class TestTypeToResolve
		{
		}
	}
}
