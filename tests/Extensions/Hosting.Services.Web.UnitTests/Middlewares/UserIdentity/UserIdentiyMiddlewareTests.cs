// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
			UserHashIdentityMiddleware middleware = GetMiddelware(saltProvider: saltProvider);

			HttpContext context = HttpContextHelper.GetContextWithIp("192.168.100.1");
			string initialHash = middleware.CreateUserHash(context);

			random.NextBytes(saltProvider.Salt);
			string changedHash = middleware.CreateUserHash(context);

			Assert.AreNotEqual(changedHash, initialHash);
		}

		[DataTestMethod]
		[DataRow(true, true, true, false)]
		[DataRow(false, true, true, true)]
		[DataRow(false, false, true, true)]
		public void CreateUserHash_CallProvidersInOrder(bool firstApplicable, bool secondApplicable, bool firstShouldBeCalled, bool secondShouldBeCalled)
		{
			HttpContext context = HttpContextHelper.GetContextWithIp("192.168.201.1");
			TestIdentityProvider mock1 = new TestIdentityProvider("Provider1", 10) { IsApplicable = firstApplicable };
			TestIdentityProvider mock2 = new TestIdentityProvider("Procider2", 15) { IsApplicable = secondApplicable };
			UserHashIdentityMiddleware middleware = GetMiddelware(saltProvider: null, mock1, mock2);

			string hash = middleware.CreateUserHash(context);
			mock1.AssertCallsAndReset(firstShouldBeCalled);
			mock2.AssertCallsAndReset(secondShouldBeCalled);

			if (!firstApplicable && !secondApplicable)
			{
				Assert.AreEqual(string.Empty, hash, "Hash should be empty when all providers not applicable");
			}
		}

		private static UserHashIdentityMiddleware GetMiddelware(ISaltProvider? saltProvider = null, params IUserIdentityProvider[] providers) =>
			new UserHashIdentityMiddleware(
				providers.Length > 0
					? providers
					: new IUserIdentityProvider[] { new IpBasedUserIdentityProvider() },
				saltProvider ?? new EmptySaltProvider());

		private class TestIdentityProvider : IUserIdentityProvider
		{
			private readonly string m_name;

			private int m_calls;

			public int MaxBytesInIdentity { get; set; }

			public bool IsApplicable { get; set; }

			public TestIdentityProvider(string name, int maxSize = 10)
			{
				m_name = name;
				m_calls = 0;
				MaxBytesInIdentity = maxSize;
				IsApplicable = true;
			}

			public UserIdentity GetUserIdentity(HttpContext httpContext)
			{
				return new UserIdentity();
			}

			public bool TryWriteBytes(HttpContext context, Span<byte> span, out int bytesWritten)
			{
				Assert.IsTrue(span.Length >= MaxBytesInIdentity, "Provided span size is too small");
				m_calls++;
				bytesWritten = IsApplicable ? MaxBytesInIdentity : 0;
				return IsApplicable;
			}

			public void AssertCallsAndReset(bool shouldBeCalled)
			{
				if (shouldBeCalled)
				{
					Assert.AreEqual(1, m_calls, $"{m_name} not called when it should be used");
				}
				else
				{
					Assert.AreEqual(0, m_calls, $"{m_name} called when it should not be used");
				}
				m_calls = 0;
			}
		}

		private class TestSaltProvider : ISaltProvider
		{
			public byte[] Salt { get; } = new byte[128];

			public void Dispose() { }
			public ReadOnlySpan<byte> GetSalt() => Salt.AsSpan();
		}
	}
}
