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
			IPAddress address1 = IPAddress.Parse("192.168.0.4");
			IPAddress address2 = IPAddress.Parse("127.0.0.4");

			(HttpContext context, HttpConnectionFeature feature) = CreateHttpContext();
			IMiddleware middleware = new UserIdentiyMiddleware(new EmptySaltProvider());

			string GetActivityUserHash(IPAddress address)
			{
				feature.RemoteIpAddress = address;
				Activity activity = new Activity("Test" + address).Start();
				middleware.InvokeAsync(context, c => Task.CompletedTask);
				activity.Stop();
				return activity.GetUserHash();
			}

			string hash1 = GetActivityUserHash(address1);
			string hash2 = GetActivityUserHash(address2);

			Assert.AreNotEqual(hash1, hash2);
			Assert.AreEqual(hash1, GetActivityUserHash(address1));
			Assert.AreEqual(hash2, GetActivityUserHash(address2));
		}

		[TestMethod]
		public void GetUserHash_UseIpAddress()
		{
			IPAddress address1 = IPAddress.Parse("192.168.0.3");
			IPAddress address2 = IPAddress.Parse("127.0.0.3");

			(HttpContext context, HttpConnectionFeature feature) = CreateHttpContext();
			UserIdentiyMiddleware middleware = new UserIdentiyMiddleware(new EmptySaltProvider());

			string GetUserHash(IPAddress address)
			{
				feature.RemoteIpAddress = address;
				return middleware.GetUserHash(context);
			}

			string hash1 = GetUserHash(address1);
			string hash2 = GetUserHash(address2);

			Assert.AreNotEqual(hash1, hash2);
			Assert.AreEqual(hash1, GetUserHash(address1));
			Assert.AreEqual(hash2, GetUserHash(address2));
		}

		[TestMethod]
		public void CreateHash_ChangedBaseOnIpAddress()
		{
			UserIdentiyMiddleware middleware = new UserIdentiyMiddleware(new EmptySaltProvider());

			IPAddress address1 = IPAddress.Parse("192.168.0.2");
			IPAddress address2 = IPAddress.Parse("127.0.0.2");

			string hash1 = middleware.CreateHash(address1);
			string hash2 = middleware.CreateHash(address2);

			Assert.AreNotEqual(hash1, hash2);
			Assert.AreEqual(hash1, middleware.CreateHash(address1));
			Assert.AreEqual(hash2, middleware.CreateHash(address2));
		}

		[TestMethod]
		public void CreateHash_UseSalt()
		{
			Random random = new Random();
			TestSaltProvider saltProvider = new TestSaltProvider();
			random.NextBytes(saltProvider.Salt);
			UserIdentiyMiddleware middleware = new UserIdentiyMiddleware(saltProvider);

			IPAddress address = IPAddress.Parse("192.168.100.1");
			string initialHash = middleware.CreateHash(address);

			random.NextBytes(saltProvider.Salt);
			string changedHash = middleware.CreateHash(address);

			Assert.AreNotEqual(changedHash, initialHash);
		}

		private static (HttpContext context, HttpConnectionFeature feature) CreateHttpContext()
		{
			HttpConnectionFeature feature = new HttpConnectionFeature();

			FeatureCollection features = new FeatureCollection();
			features.Set<IHttpConnectionFeature>(feature);

			Mock<HttpContext> contextMock = new Mock<HttpContext>();
			contextMock.SetupGet(c => c.Features).Returns(features);

			return (contextMock.Object, feature);
		}

		private class TestSaltProvider : ISaltProvider
		{
			public byte[] Salt { get; } = new byte[128];

			public void Dispose() { }
			public Span<byte> GetSalt() => Salt.AsSpan();
		}
	}
}
