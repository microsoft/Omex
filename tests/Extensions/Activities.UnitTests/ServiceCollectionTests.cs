// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	public class ServiceCollectionTests
	{
		[TestMethod]
		public void AddTimedScopes_TypesRegistered()
		{
			ITimedScopeProvider provider = new HostBuilder()
				.ConfigureServices(collection =>
				{
					collection.AddTimedScopes();
				})
				.Build()
				.Services
				.GetRequiredService<ITimedScopeProvider>();

			Assert.IsNotNull(provider);
			Assert.IsInstanceOfType(provider, typeof(TimedScopeProvider));
		}
	}
}
