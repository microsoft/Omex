// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.FeatureManagement.Tests.Extensions;

using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.Omex.FeatureManagement.Constants;
using Microsoft.Omex.FeatureManagement.Extensions;
using Microsoft.Omex.FeatureManagement.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class HttpContextExtensionsTests
{
	#region GetBrowser

	[TestMethod]
	public void GetBrowser_WhenUserAgentHeaderIsNotPresent_ReturnsEmptyString()
	{
		// ARRANGE
		DefaultHttpContext context = new();

		// ACT
		string result = context.GetBrowser();

		// ASSERT
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public void GetBrowser_WhenUserAgentIsNullEmptyOrWhitespace_ReturnsEmptyString(string? userAgent)
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[HeaderNames.UserAgent] = userAgent;

		// ACT
		string result = context.GetBrowser();

		// ASSERT
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	[DataRow("Chrome", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36")] // Chrome on Windows
	[DataRow("Chrome", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36")] // Chrome on Linux
	[DataRow("Firefox", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0")] // Firefox on Windows
	[DataRow("Internet Explorer", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko")] // Internet Explorer on Windows
	[DataRow("Microsoft Edge", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 Edg/91.0.864.59")] // Microsoft Edge on Windows
	[DataRow("Opera", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 OPR/77.0.4054.277")] // Opera on Windows
	[DataRow("Safari", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.1 Safari/605.1.15")] // Safari on macOS
	public void GetBrowser_WhenUserAgentIsKnownBrowser_ReturnsExpectedBrowser(string expectedBrowser, string userAgent)
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[HeaderNames.UserAgent] = userAgent;

		// ACT
		string result = context.GetBrowser();

		// ASSERT
		Assert.AreEqual(expectedBrowser, result);
	}

	[TestMethod]
	public void GetBrowser_WhenUserAgentIsUnknownBrowser_ReturnsEmptyString()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[HeaderNames.UserAgent] = "CustomBot/1.0";

		// ACT
		string result = context.GetBrowser();

		// ASSERT
		Assert.AreEqual(string.Empty, result);
	}

	#endregion

	#region GetDeviceType

	[TestMethod]
	public void GetDeviceType_WhenUserAgentHeaderIsNotPresent_ReturnsUnknown()
	{
		// ARRANGE
		DefaultHttpContext context = new();

		// ACT
		DeviceType result = context.GetDeviceType();

		// ASSERT
		Assert.AreEqual(DeviceType.Unknown, result);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public void GetDeviceType_WhenUserAgentIsNullEmptyOrWhitespace_ReturnsUnknown(string? userAgent)
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[HeaderNames.UserAgent] = userAgent;

		// ACT
		DeviceType result = context.GetDeviceType();

		// ASSERT
		Assert.AreEqual(DeviceType.Unknown, result);
	}

	[TestMethod]
	[DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36")] // Chrome on Windows
	[DataRow("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36")] // Chrome on Linux
	[DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0")] // Firefox on Windows
	[DataRow("Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko")] // Internet Explorer on Windows
	[DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 Edg/91.0.864.59")] // Microsoft Edge on Windows
	[DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 OPR/77.0.4054.277")] // Opera on Windows
	[DataRow("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.1 Safari/605.1.15")] // Safari on macOS
	public void GetDeviceType_WhenUserAgentIsDesktopBrowser_ReturnsDesktop(string userAgent)
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[HeaderNames.UserAgent] = userAgent;

		// ACT
		DeviceType result = context.GetDeviceType();

		// ASSERT
		Assert.AreEqual(DeviceType.Desktop, result);
	}

	[TestMethod]
	[DataRow("Mozilla/5.0 (iPhone; CPU iPhone OS 14_7_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.2 Mobile/15E148 Safari/604.1")] // iPhone
	[DataRow("Mozilla/5.0 (Linux; Android 10; SM-G975F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Mobile Safari/537.36")] // Android Phone
	[DataRow("Mozilla/5.0 (iPad; CPU OS 14_7_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.2 Mobile/15E148 Safari/604.1")] // iPad
	[DataRow("Mozilla/5.0 (Linux; Android 10; SM-T860) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36")] // Android Tablet
	[DataRow("Mozilla/5.0 (Windows Phone 10.0; Android 6.0.1; Microsoft; RM-1152) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Mobile Safari/537.36 Edge/15.15254")] // Windows Phone
	[DataRow("Mozilla/5.0 (Linux; U; Android 4.0.3; en-us; KFOT Build/IML74K) AppleWebKit/537.36 (KHTML, like Gecko) Silk/3.68 like Chrome/39.0.2171.93 Safari/537.36")] // Kindle Fire
	[DataRow("Mozilla/5.0 (BlackBerry; U; BlackBerry 9900; en) AppleWebKit/534.11+ (KHTML, like Gecko) Version/7.1.0.346 Mobile Safari/534.11+")] // BlackBerry
	public void GetDeviceType_WhenUserAgentIsMobileDevice_ReturnsMobile(string userAgent)
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[HeaderNames.UserAgent] = userAgent;

		// ACT
		DeviceType result = context.GetDeviceType();

		// ASSERT
		Assert.AreEqual(DeviceType.Mobile, result);
	}

	[TestMethod]
	public void GetDeviceType_WhenUserAgentIsUnknownBot_ReturnsUnknown()
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[HeaderNames.UserAgent] = "CustomBot/1.0";

		// ACT
		DeviceType result = context.GetDeviceType();

		// ASSERT
		Assert.AreEqual(DeviceType.Unknown, result);
	}

	#endregion

	#region GetPartner

	[TestMethod]
	public void GetPartner_WhenHeaderIsNotPresent_ReturnsEmptyString()
	{
		// ARRANGE
		DefaultHttpContext context = new();

		// ACT
		string result = context.GetPartner();

		// ASSERT
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public void GetPartner_WhenHeaderNullEmptyOrWhitespace_ReturnsEmptyString(string? partner)
	{
		// ARRANGE
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Partner] = partner;

		// ACT
		string result = context.GetPartner();

		// ASSERT
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void GetPartner_WhenHeaderIsValid_ReturnsPartner()
	{
		// ARRANGE
		const string partner = "TestPartner";
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Partner] = partner;

		// ACT
		string result = context.GetPartner();

		// ASSERT
		Assert.AreEqual(partner, result);
	}

	[TestMethod]
	public void GetPartner_WhenHeaderIncludesTrailingWhitespace_ReturnsPartnerTrimmed()
	{
		// ARRANGE
		const string partner = "TestPartner   ";
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Partner] = partner;

		// ACT
		string result = context.GetPartner();

		// ASSERT
		Assert.AreEqual("TestPartner", result);
	}

	[TestMethod]
	public void GetPartner_WhenPrefixHeaderIsValid_ReturnsPartnerFromPrefixedHeader()
	{
		// ARRANGE
		const string headerPrefix = "Prefix";
		const string partner = "TestPartner";
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Partner] = "IncorrectPartner";
		context.Request.Headers[$"{headerPrefix}-{RequestParameters.Header.Partner}"] = partner;

		// ACT
		string result = context.GetPartner(headerPrefix);

		// ASSERT
		Assert.AreEqual(partner, result);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public void GetPartner_WhenPrefixHeaderIsNullEmptyOrWhiteSpace_ReturnsPartnerFromDefaultHeader(string? headerPrefix)
	{
		// ARRANGE
		const string partner = "TestPartner";
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Partner] = partner;

		// ACT
		string result = context.GetPartner(headerPrefix);

		// ASSERT
		Assert.AreEqual(partner, result);
	}

	#endregion

	#region GetPlatform

	[TestMethod]
	public void GetPlatform_WhenHeaderIsNotPresent_ReturnsDefaultString()
	{
		// ARRANGE
		const string defaultPlatform = "DefaultPlatform";
		DefaultHttpContext context = new();

		// ACT
		string result = context.GetPlatform(null, defaultPlatform);

		// ASSERT
		Assert.AreEqual(defaultPlatform, result);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public void GetPlatform_WhenHeaderIsNullEmptyOrWhitespace_ReturnsDefaultString(string? platform)
	{
		// ARRANGE
		const string defaultPlatform = "DefaultPlatform";
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Platform] = platform;

		// ACT
		string result = context.GetPlatform(null, defaultPlatform);

		// ASSERT
		Assert.AreEqual(defaultPlatform, result);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public void GetPlatform_WhenHeaderIsNotPresentAndDefaultIsNullEmptyOrWhiteSpace_ReturnsEmptyString(string? defaultPlatform)
	{
		// ARRANGE
		DefaultHttpContext context = new();

		// ACT
		string result = context.GetPlatform(null, defaultPlatform);

		// ASSERT
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public void GetPlatform_WhenHeaderIsValid_ReturnsPlatform()
	{
		// ARRANGE
		const string platform = "TestPlatform";
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Platform] = platform;

		// ACT
		string result = context.GetPlatform();

		// ASSERT
		Assert.AreEqual(platform, result);
	}

	[TestMethod]
	public void GetPlatform_WhenHeaderIncludesTrailingWhitespace_ReturnsPlatformTrimmed()
	{
		// ARRANGE
		const string platform = "TestPlatform   ";
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Platform] = platform;

		// ACT
		string result = context.GetPlatform();

		// ASSERT
		Assert.AreEqual("TestPlatform", result);
	}

	[TestMethod]
	public void GetPlatform_WhenPrefixHeaderIsValid_ReturnsPlatformFromPrefixedHeader()
	{
		// ARRANGE
		const string headerPrefix = "Prefix";
		const string platform = "TestPlatform";
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Platform] = "IncorrectPlatform";
		context.Request.Headers[$"{headerPrefix}-{RequestParameters.Header.Platform}"] = platform;

		// ACT
		string result = context.GetPlatform(headerPrefix);

		// ASSERT
		Assert.AreEqual(platform, result);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public void GetPlatform_WhenPrefixHeaderIsNullEmptyOrWhiteSpace_ReturnsPlatformFromDefaultHeader(string? headerPrefix)
	{
		// ARRANGE
		const string platform = "TestPlatform";
		DefaultHttpContext context = new();
		context.Request.Headers[RequestParameters.Header.Platform] = platform;

		// ACT
		string result = context.GetPlatform(headerPrefix);

		// ASSERT
		Assert.AreEqual(platform, result);
	}

	#endregion

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
