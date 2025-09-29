// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests.Extensions;

using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Omex.Extensions.FeatureManagement.Constants;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class HttpContextExtensionsTests
{
	#region GetPartnerInfo

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public void GetPartnerInfo_WhenPartnerAndPlatformIsNullEmptyOrWhitespace_ReturnsDefaultPartner(string? value)
	{
		// ARRANGE
		const string defaultPartner = "DefaultPartner";
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Partner] = value;
		context.Request.Headers[RequestParameters.Header.Platform] = value;

		// ACT
		string result = context.GetPartnerInfo(null, defaultPartner);

		// ASSERT
		Assert.AreEqual(defaultPartner, result);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public void GetPartnerInfo_WhenPartnerIsNullEmptyOrWhitespaceAndPlatformHasValue_ReturnsPlatform(string? partner)
	{
		// ARRANGE
		const string platform = "TestPlatform";
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Partner] = partner;
		context.Request.Headers[RequestParameters.Header.Platform] = platform;

		// ACT
		string result = context.GetPartnerInfo();

		// ASSERT
		Assert.AreEqual(platform, result);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public void GetPartnerInfo_WhenPartnerHasValueAndPlatformIsNullEmptyOrWhitespace_ReturnsPartnerInfo(string? platform)
	{
		// ARRANGE
		const string partner = "TestPartner";
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Partner] = partner;
		context.Request.Headers[RequestParameters.Header.Platform] = platform;

		// ACT
		string result = context.GetPartnerInfo();

		// ASSERT
		Assert.AreEqual(partner, result);
	}

	[TestMethod]
	public void GetPartnerInfo_WhenPartnerAndPlatformHasValue_ReturnsPartnerInfo()
	{
		// ARRANGE
		const string partner = "TestPartner";
		const string platform = "TestPlatform";
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Partner] = partner;
		context.Request.Headers[RequestParameters.Header.Platform] = platform;

		// ACT
		string result = context.GetPartnerInfo();

		// ASSERT
		Assert.AreEqual($"{partner}/{platform}", result);
	}

	[TestMethod]
	public void GetPartnerInfo_WhenPartnerAndPlatformIncludesTrailingWhitespace_ReturnsPartnerInfoTrimmed()
	{
		// ARRANGE
		const string partner = "TestPartner   ";
		const string platform = "TestPlatform   ";
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Partner] = partner;
		context.Request.Headers[RequestParameters.Header.Platform] = platform;

		// ACT
		string result = context.GetPartnerInfo();

		// ASSERT
		Assert.AreEqual("TestPartner/TestPlatform", result);
	}

	#endregion

	#region IsLocal

	[TestMethod]
	public void IsLocal_WhenForwardedAddressIsNotNoneAndNotLoopback_ReturnsFalse()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "8.8.8.8";
		context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
		context.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");

		// ACT
		bool result = context.IsLocal();

		// ASSERT
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsLocal_WhenForwardedAddressIsLoopback_ReturnsTrue()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "127.0.0.1";
		context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
		context.Connection.LocalIpAddress = IPAddress.Parse("127.0.0.1");

		// ACT
		bool result = context.IsLocal();

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsLocal_WhenRemoteEqualsLocal_ReturnsTrue()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
		context.Connection.LocalIpAddress = IPAddress.Parse("10.0.0.1");

		// ACT
		bool result = context.IsLocal();

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsLocal_WhenRemoteAndLocalAreNull_ReturnsTrue()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Connection.RemoteIpAddress = null;
		context.Connection.LocalIpAddress = null;

		// ACT
		bool result = context.IsLocal();

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsLocal_WhenRemoteIsNotNullAndLocalIsNull_RemoteIsLoopback_ReturnsTrue()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Connection.RemoteIpAddress = IPAddress.Loopback;
		context.Connection.LocalIpAddress = null;

		// ACT
		bool result = context.IsLocal();

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsLocal_WhenRemoteIsNotNullAndLocalIsNull_RemoteIsNotLoopback_ReturnsFalse()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Connection.RemoteIpAddress = IPAddress.Parse("8.8.8.8");
		context.Connection.LocalIpAddress = null;

		// ACT
		bool result = context.IsLocal();

		// ASSERT
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsLocal_WhenForwardedIPv6IsLoopback_ReturnsTrue()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "::1";
		context.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;
		context.Connection.LocalIpAddress = IPAddress.IPv6Loopback;

		// ACT
		bool result = context.IsLocal();

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsLocal_WhenForwardedIPv6IsNotLoopback_ReturnsFalse()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "2001:db8::1";
		context.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;
		context.Connection.LocalIpAddress = IPAddress.IPv6Loopback;

		// ACT
		bool result = context.IsLocal();

		// ASSERT
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsLocal_WhenRemoteEqualsLocal_IPv6_ReturnsTrue()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		IPAddress address = IPAddress.Parse("fe80::1");
		context.Connection.RemoteIpAddress = address;
		context.Connection.LocalIpAddress = address;

		// ACT
		bool result = context.IsLocal();

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsLocal_WhenRemoteIsIPv6LoopbackAndLocalIsNull_ReturnsTrue()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;
		context.Connection.LocalIpAddress = null;

		// ACT
		bool result = context.IsLocal();

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsLocal_WhenRemoteIsIPv6AndLocalIsNull_RemoteIsNotLoopback_ReturnsFalse()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Connection.RemoteIpAddress = IPAddress.Parse("2001:db8::1234");
		context.Connection.LocalIpAddress = null;

		// ACT
		bool result = context.IsLocal();

		// ASSERT
		Assert.IsFalse(result);
	}

	#endregion

	#region GetForwardedAddress

	[TestMethod]
	public void GetForwardedAddress_WhenNoHeader_ReturnsNone()
	{
		// ARRANGE
		DefaultHttpContext context = new();

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.None, result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderIsEmpty_ReturnsNone()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = string.Empty;

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.None, result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasSingleIP_ReturnsIP()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "8.8.8.8";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.Parse("8.8.8.8"), result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasMultipleIPs_ReturnsFirstIP()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "8.8.8.8,1.2.3.4";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.Parse("8.8.8.8"), result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasIPWithPort_ReturnsIP()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "8.8.8.8:12345";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.Parse("8.8.8.8"), result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasInvalidIP_ReturnsNone()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "not-an-ip";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.None, result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasMultipleValues_ReturnsFirstValidIP()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "1.2.3.4,8.8.8.8,not-an-ip";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.Parse("1.2.3.4"), result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasIPv6WithPort_ReturnsIPv6()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "[::1]:8080";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.Parse("::1"), result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasIPv6WithoutPort_ReturnsIPv6()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "::1";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.Parse("::1"), result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasIPv6WithPortAndIPv4_ReturnsIPv6()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "[2001:db8::1]:8080,8.8.8.8";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.Parse("2001:db8::1"), result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasMultipleIPv6Addresses_ReturnsFirstIPv6()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "[2001:db8::1]:8080,[fe80::1]:1234";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.Parse("2001:db8::1"), result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasIPv6WithoutPortAndIPv4WithPort_ReturnsIPv6()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "::1,8.8.8.8:12345";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.Parse("::1"), result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasIPv6WithMultipleColons_ReturnsIPv6()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "2001:db8:85a3::8a2e:370:7334";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.Parse("2001:db8:85a3::8a2e:370:7334"), result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasWhitespaceValues_SkipsThem()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "   ,8.8.8.8";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.Parse("8.8.8.8"), result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasInvalidIPv6_ReturnsNone()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "[invalid]:8080";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.None, result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasInvalidIPv4WithPort_ReturnsNone()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "invalid:12345";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.None, result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasMultipleHeaderValues_ReturnsFirstValidFromHeaders()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = new(["not-an-ip", "[::1]:8080", "8.8.8.8:12345"]);

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		// Should return ::1 from the second header value, as it's the first valid encountered scanning in forward order.
		Assert.AreEqual(IPAddress.Parse("::1"), result);
	}

	#endregion
}
