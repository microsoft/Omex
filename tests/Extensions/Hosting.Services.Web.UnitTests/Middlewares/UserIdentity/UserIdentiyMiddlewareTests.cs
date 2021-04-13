// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
		public async Task CreateUserHash_UseSalt()
		{
			Random random = new Random();
			TestSaltProvider saltProvider = new TestSaltProvider();
			random.NextBytes(saltProvider.Salt);
			UserHashIdentityMiddleware middleware = GetMiddelware(saltProvider: saltProvider);

			HttpContext context = HttpContextHelper.GetContextWithIp("192.168.100.1");
			string initialHash = await middleware.CreateUserHashAsync(context).ConfigureAwait(false);

			random.NextBytes(saltProvider.Salt);
			string changedHash = await middleware.CreateUserHashAsync(context).ConfigureAwait(false);

			Assert.AreNotEqual(changedHash, initialHash);
		}

		[TestMethod]
		public async Task CreateUserHash_HandleConcurrentRequestProperly()
		{
			HttpContext[] contexts = new []
			{
				HttpContextHelper.GetContextWithIp("192.168.132.17"),
				HttpContextHelper.GetContextWithIp("192.168.132.18"),
				HttpContextHelper.GetContextWithIp("192.168.132.19")
			};

			TestIdentityProvider provider = new ("ProviderWrapper", new IpBasedUserIdentityProvider());
			UserHashIdentityMiddleware middleware = GetMiddelware(saltProvider: null, provider);

			ParallelQuery<Task<(int contextIndex, string hash)>> result = Enumerable.Range(0, 100).AsParallel()
				.Select(async index =>
				{
					int contextIndex = index % contexts.Length;
					string hash = await middleware.CreateUserHashAsync(contexts[contextIndex]).ConfigureAwait(false);
					return (contextIndex, hash);
				});
			(int contextIndex, string hash)[] tupleList = await Task.WhenAll(result).ConfigureAwait(false);
			IEnumerable<int> uniqueHases = tupleList.GroupBy(hashIndexPair =>
				hashIndexPair.contextIndex)
				.Select(hashesPerContext => hashesPerContext.Distinct().Count());
			foreach (int uniquHashesPerContext in uniqueHases)
			{
				Assert.AreEqual(1, uniquHashesPerContext, "Hashes for the same context should be identical");
			}
		}

		[DataTestMethod]
		[DataRow(true, true, true, false)]
		[DataRow(false, true, true, true)]
		[DataRow(false, false, true, true)]
		public async Task CreateUserHash_CallProvidersInOrder(bool firstApplicable, bool secondApplicable, bool firstShouldBeCalled, bool secondShouldBeCalled)
		{
			HttpContext context = HttpContextHelper.GetContextWithIp("192.168.201.1");
			TestIdentityProvider mock1 = new ("Provider1", 15) { IsApplicable = firstApplicable };
			TestIdentityProvider mock2 = new ("Procider2", 10) { IsApplicable = secondApplicable };
			UserHashIdentityMiddleware middleware = GetMiddelware(saltProvider: null, mock1, mock2);

			string hash = await middleware.CreateUserHashAsync(context).ConfigureAwait(false);
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

			private readonly IUserIdentityProvider? m_provider;

			private int m_calls;

			public int MaxBytesInIdentity { get; set; }

			public bool IsApplicable { get; set; }

			public TestIdentityProvider(string name, IUserIdentityProvider provider)
				: this(name, provider.MaxBytesInIdentity) =>
					m_provider = provider;

			public TestIdentityProvider(string name, int maxSize = 10, IUserIdentityProvider? provider = null)
			{
				m_name = name;
				m_calls = 0;
				MaxBytesInIdentity = maxSize;
				m_provider = provider;
				IsApplicable = true;
			}

			public Task<bool> TryWriteBytesAsync(HttpContext context, Span<byte> span, out int bytesWritten)
			{
				Assert.AreEqual(MaxBytesInIdentity, span.Length, "Wrond span size provided");
				m_calls++;
				bytesWritten = IsApplicable ? MaxBytesInIdentity : 0;

				foreach (byte val in span)
				{
					if (val != 0)
					{
						Assert.Fail($"Non zero byte value was passed into {nameof(TryWriteBytesAsync)}");
					}
				}

				if (m_provider != null)
				{
					return m_provider.TryWriteBytesAsync(context, span, out bytesWritten);
				}

				return Task.FromResult(IsApplicable);
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
