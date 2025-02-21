// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares.UserIdentity.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests
{
	[TestClass]
	public class StaticSaltProviderTests
	{
		[TestMethod]
		public void GetSalt_ReturnsConsistentResult()
		{
			IOptions<StaticSaltProviderOptions> options1 = Options.Create(new StaticSaltProviderOptions
			{
				SaltValue = "testSaltString1"
			});

			IOptions<StaticSaltProviderOptions> options2 = Options.Create(new StaticSaltProviderOptions
			{
				SaltValue = "testSaltString2"
			});

			ISaltProvider provider1 = new StaticSaltProvider(options1);
            ISaltProvider provider2 = new StaticSaltProvider(options1);
            ISaltProvider provider3 = new StaticSaltProvider(options2);

			CollectionAssert.AreEqual(provider1.GetSalt().ToArray(), provider2.GetSalt().ToArray());
            CollectionAssert.AreNotEqual(provider1.GetSalt().ToArray(), provider3.GetSalt().ToArray());
		}
	}
}
