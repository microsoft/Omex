// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests.Filters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement.Constants;
using Microsoft.Omex.Extensions.FeatureManagement.Filters;
using Microsoft.Omex.Extensions.FeatureManagement.Filters.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class IPAddressFilterTests
{
	private const string TestFeatureName = "TestFeature";
	private readonly Mock<IHttpContextAccessor> m_httpContextAccessorMock;
	private readonly Mock<IIPRangeProvider> m_ipRangeProviderMock;
	private readonly Mock<ILogger<IPAddressFilter>> m_loggerMock;
	private readonly IPAddressFilter m_filter;
	private readonly FeatureFilterEvaluationContext m_context;
	private readonly DefaultHttpContext m_httpContext;

	public IPAddressFilterTests()
	{
		m_httpContextAccessorMock = new();
		m_ipRangeProviderMock = new();
		m_loggerMock = new();

		// Setup IP ranges for tests.
		IPNetwork[] ipRanges =
		[
			IPNetwork.Parse("10.0.0.0/8"),
			IPNetwork.Parse("172.16.0.0/12"),
		];

		m_ipRangeProviderMock
			.Setup(x => x.GetIPRanges("TESTIPS"))
			.Returns(ipRanges);
		m_ipRangeProviderMock
			.Setup(x => x.GetIPRanges("testips"))
			.Returns(ipRanges);

		m_filter = new(m_httpContextAccessorMock.Object, m_ipRangeProviderMock.Object, m_loggerMock.Object);
		m_httpContext = new();
		m_httpContextAccessorMock.Setup(x => x.HttpContext).Returns(m_httpContext);
		m_context = new()
		{
			FeatureName = TestFeatureName,
			Parameters = new ConfigurationBuilder().Build(),
		};
	}

	#region EvaluateAsync

	[TestMethod]
	public async Task EvaluateAsync_WhenAllowedRangeIsNull_ReturnsFalse()
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "AllowedRange", string.Empty },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenHttpContextIsNull_ReturnsFalse()
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "AllowedRange", "TESTIPS" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;
		m_httpContextAccessorMock.Setup(h => h.HttpContext).Returns((HttpContext?)null);

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenIpIsLocal_ReturnsTrue()
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "AllowedRange", "TESTIPS" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		m_httpContext.Request.Headers[RequestParameters.Header.ForwardedFor] = "127.0.0.1";

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenRemoteIpAddressIsNull_ReturnsFalse()
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "AllowedRange", "TESTIPS" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		m_httpContext.Request.Headers[RequestParameters.Header.ForwardedFor] = IPAddress.None.ToString();

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenIpIsInRange_ReturnsTrue()
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "AllowedRange", "TESTIPS" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// Use one of the test IPs from the static list.
		IPAddress testIp = IPAddress.Parse("10.1.1.1");
		m_httpContext.Request.Headers[RequestParameters.Header.ForwardedFor] = testIp.ToString();

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
		VerifyIPRangeLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenIpIsNotInRange_ReturnsFalse()
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "AllowedRange", "TESTIPS" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// Use a non-test IP.
		IPAddress nonTestIp = IPAddress.Parse("203.0.113.10");
		m_httpContext.Request.Headers[RequestParameters.Header.ForwardedFor] = nonTestIp.ToString();

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
		VerifyIPRangeLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenAllowedRangeIsNotTestIPs_ReturnsFalse()
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "AllowedRange", "SomeOtherRange" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;
		m_ipRangeProviderMock.Setup(x => x.GetIPRanges("SomeOtherRange")).Returns((IPNetwork[]?)null);

		// Use a test IP, but the range is not set to TESTIPS.
		IPAddress testIp = IPAddress.Parse("10.1.1.1");
		m_httpContext.Request.Headers[RequestParameters.Header.ForwardedFor] = testIp.ToString();

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenCustomIPRangeIsConfigured_ReturnsTrue()
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "AllowedRange", "CUSTOMRANGE" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// Setup custom IP range.
		IPNetwork[] customRanges =
		[
			IPNetwork.Parse("10.0.0.0/8"),
			IPNetwork.Parse("192.168.0.0/16"),
		];

		m_ipRangeProviderMock.Setup(x => x.GetIPRanges("CUSTOMRANGE")).Returns(customRanges);

		// Use an IP that is in the custom range.
		IPAddress customRangeIp = IPAddress.Parse("10.1.1.1");
		m_httpContext.Request.Headers[RequestParameters.Header.ForwardedFor] = customRangeIp.ToString();

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
		VerifyIPRangeLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenUsingForwardedAddress_ChecksForwardedAddressFirst()
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "AllowedRange", "TESTIPS" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// Setup a forwarded address that is in the test range.
		IPAddress forwardedIp = IPAddress.Parse("10.1.1.1");
		m_httpContext.Request.Headers[RequestParameters.Header.ForwardedFor] = forwardedIp.ToString();
		m_httpContext.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.10");

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
		VerifyIPRangeLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenAllowedRangeIsTestIPsCaseInsensitive_ReturnsTrue()
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "AllowedRange", "testips" }, // lowercase
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		IPAddress testIp = IPAddress.Parse("10.1.1.1");
		m_httpContext.Request.Headers[RequestParameters.Header.ForwardedFor] = testIp.ToString();

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
		VerifyIPRangeLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenProviderReturnsEmptyArray_ReturnsFalse()
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "AllowedRange", "EMPTYRANGE" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;
		m_ipRangeProviderMock.Setup(x => x.GetIPRanges("EMPTYRANGE")).Returns([]);

		// Use any IP address.
		IPAddress testIp = IPAddress.Parse("203.0.113.10");
		m_httpContext.Request.Headers[RequestParameters.Header.ForwardedFor] = testIp.ToString();

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
		VerifyIPRangeLogging(false);
	}

	[TestMethod]
	[DataRow("10.1.1.1")]
	[DataRow("203.0.113.10")]
	[DataRow("127.0.0.1")]
	public async Task EvaluateAsync_WhenCalled_DoesNotLogIPAddress(string ipAddress)
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "AllowedRange", "TESTIPS" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;
		m_httpContext.Request.Headers[RequestParameters.Header.ForwardedFor] = ipAddress;

		// ACT
		await m_filter.EvaluateAsync(m_context);

		// ASSERT
		// Ensure that none of the logged messages contain the literal IP address.
		IEnumerable<string> loggedMessages = m_loggerMock.Invocations
			.Select(i => i.Arguments[2]?.ToString() ?? string.Empty);
		foreach (string message in loggedMessages)
		{
			Assert.IsFalse(message.Contains(ipAddress, StringComparison.Ordinal), $"Log message should not contain IP address '{ipAddress}', but was: '{message}'.");
		}
	}

	#endregion

	private void VerifyLogging(bool expectedIsEnabled) =>
		m_loggerMock.Verify(
			logger => logger.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), $"IPAddressFilter returning '{expectedIsEnabled}' for '{TestFeatureName}'.", StringComparison.Ordinal)),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);

	private void VerifyIPRangeLogging(bool isAllowed)
	{
		string verb = isAllowed ? "allowed" : "blocked";
		m_loggerMock.Verify(
			logger => logger.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), $"IPAddressFilter '{verb}' access to '{TestFeatureName}'.", StringComparison.Ordinal)),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
	}
}
