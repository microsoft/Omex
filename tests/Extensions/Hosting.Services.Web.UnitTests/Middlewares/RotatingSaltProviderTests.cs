// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.Internal;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests
{
	[TestClass]
	public class RotatingSaltProviderTests
	{
		[TestMethod]
		public void GetSalt_RotatesEarlierThen48Hours()
		{
			Mock<ISystemClock> systemClock = new Mock<ISystemClock>();
			systemClock
				.SetupGet(c => c.UtcNow)
				.Returns(DateTimeOffset.Now);

			ISaltProvider provider = new RotatingSaltProvider(systemClock.Object);

			byte[] intialSalt = provider.GetSalt().ToArray();

			systemClock
				.SetupGet(c => c.UtcNow)
				.Returns(DateTimeOffset.Now.AddHours(20));

			CollectionAssert.AreEqual(intialSalt, provider.GetSalt().ToArray());

			systemClock
				.SetupGet(c => c.UtcNow)
				.Returns(DateTimeOffset.Now.AddHours(47));

			CollectionAssert.AreNotEqual(intialSalt, provider.GetSalt().ToArray());
		}
	}
}
