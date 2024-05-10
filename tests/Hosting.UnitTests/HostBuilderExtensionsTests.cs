// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Hosting.Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.UnitTests
{
	[TestClass]
	public class HostBuilderExtensionsTests
	{
		[DataTestMethod]
		[DataRow(typeof(ILogger<HostBuilderExtensionsTests>))]
		[DataRow(typeof(ActivitySource))]
		[Obsolete("AddOmexServices is Obsolete and pending for removal on 1 July 2024. Code: 8913598.")]
		public void AddOmexServices_TypesRegistered(Type type)
		{
			object? collectionObj = new ServiceCollection()
				.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build()) // Added IConfiguration because one of the dependency depends on IOptions which in turn depends on IConfiguration
				.AddSingleton<IHostEnvironment>(new HostingEnvironment())
				.AddOmexServices()
				.BuildServiceProvider(new ServiceProviderOptions
				{
					ValidateOnBuild = true,
					ValidateScopes = true
				}).GetService(type);

			Assert.IsNotNull(collectionObj, FormattableString.Invariant($"Type {type} was not resolved after AddOmexServices to ServiceCollection"));

			object hostObj = new HostBuilder()
				.AddOmexServices()
				.UseDefaultServiceProvider(options =>
				{
					options.ValidateOnBuild = true;
					options.ValidateScopes = true;
				})
				.Build().Services.GetRequiredService(type);

			Assert.IsNotNull(hostObj, FormattableString.Invariant($"Type {type} was not resolved after AddOmexServices to HostBuilder"));
		}

		[TestMethod]
		public void AddCertificateReader_TypesRegistered()
		{
			ICertificateReader? collectionObj = new ServiceCollection()
				.AddCertificateReader()
				.AddLogging()
				.BuildServiceProvider(new ServiceProviderOptions
				{
					ValidateOnBuild = true,
					ValidateScopes = true
				})
				.GetService<ICertificateReader>();

			Assert.IsNotNull(collectionObj, FormattableString.Invariant($"Type {nameof(ICertificateReader)} was not resolved after AddCertificateReader to ServiceCollection"));
		}
	}
}
