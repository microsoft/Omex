// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	public class ServiceCollectionTests
	{
		[TestMethod]
		public void AddTimedScopes_TypesRegistered()
		{
			ActivitySource provider = new HostBuilder()
				.ConfigureServices(collection =>
				{
					collection.AddOmexActivitySource();
				})
				.Build()
				.Services
				.GetRequiredService<ActivitySource>();

			Assert.IsNotNull(provider);
		}
	}
}
