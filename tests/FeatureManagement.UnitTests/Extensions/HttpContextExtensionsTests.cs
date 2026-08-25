// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests.Extensions;

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
}
