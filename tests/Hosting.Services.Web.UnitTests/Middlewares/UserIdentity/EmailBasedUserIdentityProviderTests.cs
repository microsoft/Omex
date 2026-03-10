// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests
{
	[TestClass]
	public class EmailBasedUserIdentityProviderTests
	{
		[TestMethod]
		public async Task TryWriteBytes_NoEmail_ReturnFalse()
		{
			IUserIdentityProvider provider = new EmailBasedUserIdentityProvider(new NullLogger<EmailBasedUserIdentityProvider>());
			(HttpContext context, _) = HttpContextHelper.CreateHttpContext();
			Memory<byte> memory = new byte[provider.MaxBytesInIdentity];
			(bool result, int bytesWritten) = await provider.TryWriteBytesAsync(context, memory);
			Assert.IsFalse(result);
			Assert.AreEqual(-1, bytesWritten);
		}

		[TestMethod]
		public async Task TryWriteBytes_DifferentEmail_HashValueChanged()
		{
			IUserIdentityProvider provider = new EmailBasedUserIdentityProvider(new NullLogger<EmailBasedUserIdentityProvider>());

			HttpContext context1 = HttpContextHelper.GetContextWithEmail("Abc123@outlook.com");
			HttpContext context2 = HttpContextHelper.GetContextWithEmail("Abc456@gmail.com");

			byte[] hash1 = await GetIdentityAsync(provider, context1);
			byte[] hash2 = await GetIdentityAsync(provider, context2);

			context1.Request.Body.Close();
			context2.Request.Body.Close();

			CollectionAssert.AreNotEqual(hash1, hash2);

			HttpContext context3 = HttpContextHelper.GetContextWithEmail("Abc123@outlook.com");
			HttpContext context4 = HttpContextHelper.GetContextWithEmail("Abc456@gmail.com");

			CollectionAssert.AreEqual(hash1, await GetIdentityAsync(provider, context3));
			CollectionAssert.AreEqual(hash2, await GetIdentityAsync(provider, context4));

			context3.Request.Body.Close();
			context4.Request.Body.Close();
		}

		private async Task<byte[]> GetIdentityAsync(IUserIdentityProvider provider, HttpContext context)
		{
			Memory<byte> memory = new byte[provider.MaxBytesInIdentity];
			(_, int bytes) = await provider.TryWriteBytesAsync(context, memory);
			Assert.IsGreaterThanOrEqualTo(bytes, provider.MaxBytesInIdentity, "Written size bigger then max size");
			return memory.Span.ToArray();
		}
	}
}
