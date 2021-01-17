// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
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

	[TestClass]
	public class ServiceCollectionTests
	{
		[TestMethod]
		public void AddMiddlewares_RegisterUserIdentiyMiddleware()
		{
			UserIdentiyMiddleware middleware = new HostBuilder()
				.ConfigureServices((context, collection) =>
				{
					collection.AddOmexMiddleware();
				})
				.Build().Services
				.GetRequiredService<UserIdentiyMiddleware>();

			Assert.IsNotNull(middleware);
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
						options.LoggingComlience = UserIdentiyComplianceLevel.LiveId;
					})
					.AddOmexMiddleware();
				})
				.Build().Services
				.GetRequiredService<ISaltProvider>();

			Assert.IsInstanceOfType(saltProvider, typeof(EmptySaltProvider));
		}
	}
}
