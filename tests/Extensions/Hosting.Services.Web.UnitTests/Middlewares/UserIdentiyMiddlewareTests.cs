// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests
{
	[TestClass]
	public class UserIdentiyMiddlewareTests
	{
		[TestMethod]
		public void InvokeAsync_SetsActivityPropetry()
		{
			HttpContext context1 = HttpContextHelper.GetContextWithIp("192.168.0.4");
			HttpContext context2 = HttpContextHelper.GetContextWithIp("127.0.0.4");

			IMiddleware middleware = GetMiddelware();

			string GetActivityUserHash(HttpContext context)
			{
				Activity activity = new Activity(nameof(InvokeAsync_SetsActivityPropetry)).Start();
				middleware.InvokeAsync(context, c => Task.CompletedTask);
				activity.Stop();
				return activity.GetUserHash();
			}

			string hash1 = GetActivityUserHash(context1);
			string hash2 = GetActivityUserHash(context2);

			Assert.AreNotEqual(hash1, hash2);
			Assert.AreEqual(hash1, GetActivityUserHash(context1));
			Assert.AreEqual(hash2, GetActivityUserHash(context2));
		}

		[TestMethod]
		public void CreateUserHash_UseSalt()
		{
			Random random = new Random();
			TestSaltProvider saltProvider = new TestSaltProvider();
			random.NextBytes(saltProvider.Salt);
			UserIdentiyMiddleware middleware = GetMiddelware(saltProvider);

			HttpContext context = HttpContextHelper.GetContextWithIp("192.168.100.1");
			string initialHash = middleware.CreateUserHash(context);

			random.NextBytes(saltProvider.Salt);
			string changedHash = middleware.CreateUserHash(context);

			Assert.AreNotEqual(changedHash, initialHash);
		}

		private static UserIdentiyMiddleware GetMiddelware(ISaltProvider? saltProvider = null) =>
			new UserIdentiyMiddleware(new IpBasedUserIdentityProvider(), saltProvider ?? new EmptySaltProvider());

		private class TestSaltProvider : ISaltProvider
		{
			public byte[] Salt { get; } = new byte[128];

			public void Dispose() { }
			public ReadOnlySpan<byte> GetSalt() => Salt.AsSpan();
		}
	}
}
