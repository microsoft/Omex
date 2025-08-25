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
	public void GetForwardedAddress_WhenHeaderHasSingleIp_ReturnsIp()
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
	public void GetForwardedAddress_WhenHeaderHasMultipleIps_ReturnsLastIp()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "8.8.8.8,1.2.3.4";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.Parse("1.2.3.4"), result);
	}

	[TestMethod]
	public void GetForwardedAddress_WhenHeaderHasIpWithPort_ReturnsIp()
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
	public void GetForwardedAddress_WhenHeaderHasInvalidIp_ReturnsNone()
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
	public void GetForwardedAddress_WhenHeaderHasMultipleValues_ReturnsLastValidIp()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.ForwardedFor] = "1.2.3.4,8.8.8.8,not-an-ip";

		// ACT
		IPAddress result = context.GetForwardedAddress();

		// ASSERT
		Assert.AreEqual(IPAddress.Parse("8.8.8.8"), result);
	}

	#endregion
}
