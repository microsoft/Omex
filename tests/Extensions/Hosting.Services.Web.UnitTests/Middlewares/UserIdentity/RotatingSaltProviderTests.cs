// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging.Abstractions;
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

			ISaltProvider provider = new RotatingSaltProvider(systemClock.Object, new NullLogger<RotatingSaltProvider>());

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

		[TestMethod]
		public void GetSalt_ReturnsConsistentResult()
		{
			ISaltProvider provider = new RotatingSaltProvider(new SystemClock(), new NullLogger<RotatingSaltProvider>());

			byte[][] salts = Enumerable.Range(0, 12)
				.AsParallel()
				.Select(i => provider.GetSalt().ToArray())
				.ToArray();

			byte[] sample = provider.GetSalt().ToArray();

			foreach (byte[] salt in salts)
			{
				CollectionAssert.AreEqual(sample, salt);
			}
		}
	}
}
