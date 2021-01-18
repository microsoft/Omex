// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests.Internal
{
	[TestClass]
	public class ServiceCollectionExtensionsTests
	{
		[TestMethod]
		public void AddOmexMiddleware_RegisterTypes()
		{
			IServiceProvider provider = new ServiceCollection()
				.AddSingleton(new Mock<IExecutionContext>().Object)
				.AddOmexMiddleware()
				.BuildServiceProvider();

			Assert.IsNotNull(provider.GetRequiredService<ActivityEnrichmentMiddleware>());
			Assert.IsNotNull(provider.GetRequiredService<ResponseHeadersMiddleware>());
#pragma warning disable CS0618
			Assert.IsNotNull(provider.GetRequiredService<ObsoleteCorrelationHeadersMiddleware>());
#pragma warning restore CS0618
		}
	}
}
