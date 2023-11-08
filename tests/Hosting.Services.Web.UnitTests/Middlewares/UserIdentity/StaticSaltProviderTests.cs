// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Text;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests
{
	[TestClass]
	public class StaticSaltProviderTests
	{
		[TestMethod]
		public void GetSalt_ReturnsConsistentResult()
		{
            string saltString1 = "testSaltString1";
            string saltString2 = "testSaltString2";

			ISaltProvider provider1 = new StaticSaltProvider(saltString1);
            ISaltProvider provider2 = new StaticSaltProvider(saltString1);
            ISaltProvider provider3 = new StaticSaltProvider(saltString2);

			CollectionAssert.AreEqual(provider1.GetSalt().ToArray(), provider2.GetSalt().ToArray());
            CollectionAssert.AreNotEqual(provider1.GetSalt().ToArray(), provider3.GetSalt().ToArray());
		}
	}
}
