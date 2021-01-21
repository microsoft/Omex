// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests.Internal
{
	[TestClass]
	public class ServiceCollectionExtensionsTests
	{
		[DataTestMethod]
		[DataRow(typeof(ActivityEnrichmentMiddleware))]
		[DataRow(typeof(ResponseHeadersMiddleware))]
		[DataRow(typeof(UserIdentiyMiddleware))]
#pragma warning disable CS0618
		[DataRow(typeof(ObsoleteCorrelationHeadersMiddleware))]
#pragma warning restore CS0618
		public void AddOmexMiddleware_RegisterTypes(Type type)
		{
			IServiceProvider provider = new HostBuilder()
				.ConfigureServices((context, collection) =>
				{
					collection
						.AddSingleton(new Mock<IExecutionContext>().Object)
						.AddOmexMiddleware();
				})
				.UseDefaultServiceProvider(configure =>
				{
					configure.ValidateOnBuild = true;
					configure.ValidateScopes = true;
				})
				.Build().Services;

			Assert.IsNotNull(provider.GetRequiredService(type));
		}

		[TestMethod]
		public void AddMiddlewares_RegistersRotatedSaltProviderByDefault()
		{
			ISaltProvider saltProvider = new HostBuilder()
				.ConfigureServices((context, collection) =>
				{
					collection.AddOmexMiddleware();
				})
				.Build().Services
				.GetRequiredService<ISaltProvider>();

			Assert.IsInstanceOfType(saltProvider, typeof(RotatingSaltProvider));
		}

		[TestMethod]
		public void AddMiddlewares_RegistersEmptySaltProviderForLiveId()
		{
			ISaltProvider saltProvider = new HostBuilder()
				.ConfigureServices((context, collection) =>
				{
					collection
					.Configure<UserIdentiyMiddlewareOptions>(options =>
					{
						options.LoggingCompliance = UserIdentiyComplianceLevel.LiveId;
					})
					.AddOmexMiddleware();
				})
				.Build().Services
				.GetRequiredService<ISaltProvider>();

			Assert.IsInstanceOfType(saltProvider, typeof(EmptySaltProvider));
		}
	}
}
