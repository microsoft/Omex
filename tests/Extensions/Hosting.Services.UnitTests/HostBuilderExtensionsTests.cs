using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.Omex.Extensions.TimedScopes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hosting.Services.UnitTests
{
	[TestClass]
	public class HostBuilderExtensionsTests
	{
		[DataTestMethod]
		[DataRow(typeof(IServiceContext), typeof(OmexServiceFabricContext))]
		[DataRow(typeof(IMachineInformation), typeof(ServiceFabricMachineInformation))]
		[DataRow(typeof(ITimedScopeProvider), null)]
		[DataRow(typeof(ILogger), null)]
		public void TestOmexTypeRegistration(Type typeToResolver, Type? expectedImplementationType)
		{
			object obj = new ServiceCollection()
				.AddOmexServiceFabricDependencies()
				.BuildServiceProvider()
				.GetServices(typeToResolver);

			Assert.IsNotNull(obj);

			if (expectedImplementationType != null)
			{
				Assert.IsInstanceOfType(obj, expectedImplementationType);
			}
		}
	}
}
