// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests
{
	[TestClass]
	public class IpBasedUserIdentityProviderTests
	{
		[TestMethod]
		public void GetUserIdentity_ReturnEmptyUserIfNoIpAddress()
		{
			IUserIdentityProvider provider = new IpBasedUserIdentityProvider();
			(HttpContext context, _) = HttpContextHelper.CreateHttpContext();

			UserIdentity result = provider.GetUserIdentity(context);
			Assert.AreEqual(result.User, string.Empty);
			Assert.AreEqual(result.UserHashType, UserIdentifierType.IpAddress);
		}

		[TestMethod]
		public void GetUserIdentity_ChangedBaseOnIpAddress()
		{
			const string ipAddr1 = "192.168.0.2";
			const string ipAddr2 = "127.0.0.2";

			IUserIdentityProvider provider = new IpBasedUserIdentityProvider();

			HttpContext context1 = HttpContextHelper.GetContextWithIp(ipAddr1);
			HttpContext context2 = HttpContextHelper.GetContextWithIp(ipAddr2);

			UserIdentity user1 = provider.GetUserIdentity(context1);
			UserIdentity user2 = provider.GetUserIdentity(context2);

			Assert.AreNotEqual(user1.User, user2.User);
			Assert.AreEqual(user1.User, ipAddr1);
			Assert.AreEqual(user1.UserHashType, UserIdentifierType.IpAddress);
			Assert.AreEqual(user2.User, ipAddr2);
			Assert.AreEqual(user2.UserHashType, UserIdentifierType.IpAddress);
		}

		[TestMethod]
		public void TryWriteBytes_ReturnFalseIfNoIpAddress()
		{
			IUserIdentityProvider provider = new IpBasedUserIdentityProvider();
			(HttpContext context, _) = HttpContextHelper.CreateHttpContext();

			bool result = provider.TryWriteBytes(context, new byte[provider.MaxBytesInIdentity].AsSpan(), out int bytesWritten);
			Assert.IsFalse(result);
			Assert.AreEqual(-1, bytesWritten);
		}

		[TestMethod]
		public void TryWriteBytes_ChangedBaseOnIpAddress()
		{
			IUserIdentityProvider provider = new IpBasedUserIdentityProvider();

			HttpContext context1 = HttpContextHelper.GetContextWithIp("192.168.0.2");
			HttpContext context2 = HttpContextHelper.GetContextWithIp("127.0.0.2");

			byte[] hash1 = GetIdentity(provider, context1);
			byte[] hash2 = GetIdentity(provider, context2);

			CollectionAssert.AreNotEqual(hash1, hash2);
			CollectionAssert.AreEqual(hash1, GetIdentity(provider, context1));
			CollectionAssert.AreEqual(hash2, GetIdentity(provider, context2));
		}

		private static byte[] GetIdentity(IUserIdentityProvider provider, HttpContext context)
		{
			byte[] array = new byte[provider.MaxBytesInIdentity];
			provider.TryWriteBytes(context, array.AsSpan(), out int bytes);
			Assert.IsTrue(provider.MaxBytesInIdentity >= bytes, "Written size bigger then max size");
			return array;
		}
	}
}
