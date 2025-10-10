// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests
{
	[TestClass]
	public class IpBasedUserIdentityProviderTests
	{
		[TestMethod]
		public async Task TryWriteBytes_ReturnFalseIfNoIpAddress()
		{
			IUserIdentityProvider provider = new IpBasedUserIdentityProvider();
			(HttpContext context, _) = HttpContextHelper.CreateHttpContext();
			Memory<byte> memory = new byte[provider.MaxBytesInIdentity];
			(bool result, int bytesWritten) = await provider.TryWriteBytesAsync(context, memory).ConfigureAwait(false);
			Assert.IsFalse(result);
			Assert.AreEqual(-1, bytesWritten);
		}

		[TestMethod]
		public async Task TryWriteBytes_ChangedBaseOnIpAddress()
		{
			IUserIdentityProvider provider = new IpBasedUserIdentityProvider();

			HttpContext context1 = HttpContextHelper.GetContextWithIp("192.168.0.2");
			HttpContext context2 = HttpContextHelper.GetContextWithIp("127.0.0.2");

			byte[] hash1 = await GetIdentityAsync(provider, context1).ConfigureAwait(false);
			byte[] hash2 = await GetIdentityAsync(provider, context2).ConfigureAwait(false);

			CollectionAssert.AreNotEqual(hash1, hash2);
			CollectionAssert.AreEqual(hash1, await GetIdentityAsync(provider, context1).ConfigureAwait(false));
			CollectionAssert.AreEqual(hash2, await GetIdentityAsync(provider, context2).ConfigureAwait(false));
		}

		private async Task<byte[]> GetIdentityAsync(IUserIdentityProvider provider, HttpContext context)
		{
			Memory<byte> memory = new byte[provider.MaxBytesInIdentity];
			(_, int bytes) = await provider.TryWriteBytesAsync(context, memory).ConfigureAwait(false);
			Assert.IsGreaterThanOrEqualTo(bytes, provider.MaxBytesInIdentity, "Written size bigger then max size");
			return memory.Span.ToArray();
		}
	}
}
