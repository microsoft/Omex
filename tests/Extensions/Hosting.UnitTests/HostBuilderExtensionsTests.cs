using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.TimedScopes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.UnitTests
{
	[TestClass]
	public class HostBuilderExtensionsTests
	{
		[DataTestMethod]
		[DataRow(typeof(ILogger<HostBuilderExtensionsTests>))]
		[DataRow(typeof(ITimedScopeProvider))]
		public void CheckThatRequiredTypesRegistred(Type type)
		{
			object collectionObj = new ServiceCollection()
				.AddSingleton<IHostEnvironment>(new HostingEnvironment())
				.AddOmexServices()
				.BuildServiceProvider(new ServiceProviderOptions
				{
					ValidateOnBuild = true,
					ValidateScopes = true
				}).GetService(type);

			Assert.IsNotNull(collectionObj, $"Type {type} was not resolved after AddOmexServices to ServiceCollection");

			object hostObj = Host
				.CreateDefaultBuilder()
				.AddOmexServices()
				.UseDefaultServiceProvider(options =>
				{
					options.ValidateOnBuild = true;
					options.ValidateScopes = true;
				})
				.Build().Services.GetService(type);

			Assert.IsNotNull(hostObj, $"Type {type} was not resolved after AddOmexServices to HostBuilder");
		}
	}
}
