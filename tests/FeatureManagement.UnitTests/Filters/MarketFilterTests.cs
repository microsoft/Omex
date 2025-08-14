// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.FeatureManagement.Tests.Filters;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Omex.FeatureManagement.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class MarketFilterTests
{
	private const string TestFeatureName = "TestFeature";
	private readonly Mock<IHttpContextAccessor> m_httpContextAccessorMock;
	private readonly Mock<ILogger<MarketFilter>> m_loggerMock;
	private readonly MarketFilter m_filter;
	private readonly FeatureFilterEvaluationContext m_context;
	private readonly HttpContext m_httpContext;

	public MarketFilterTests()
	{
		m_httpContextAccessorMock = new();
		m_loggerMock = new();
		m_filter = new(m_httpContextAccessorMock.Object, m_loggerMock.Object);
		m_httpContext = new DefaultHttpContext();
		m_httpContextAccessorMock.Setup(x => x.HttpContext).Returns(m_httpContext);
		m_context = new()
		{
			FeatureName = TestFeatureName,
			Parameters = new ConfigurationBuilder().Build(),
		};
	}

	#region EvaluateAsync

	[TestMethod]
	public async Task EvaluateAsync_WhenMarketIsAllowed_ReturnsTrue()
	{
		// ARRANGE
		const string marketName = "TestMarket";
		m_httpContext.Request.QueryString = new($"?market={marketName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", marketName },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyIncludedLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenMarketIsNotAllowed_ReturnsFalse()
	{
		// ARRANGE
		const string marketName = "TestMarket";
		const string allowedMarket = "AllowedMarket";
		m_httpContext.Request.QueryString = new($"?market={marketName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", allowedMarket },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyIncludedLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenNoMarketInQuery_ReturnsFalse()
	{
		// ARRANGE
		const string allowedMarket = "AllowedMarket";
		m_httpContext.Request.QueryString = new(string.Empty);

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", allowedMarket },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyIncludedLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenHttpContextIsNull_ReturnsFalse()
	{
		// ARRANGE
		m_httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", "AllowedMarket" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyIncludedLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenNoAllowedMarkets_ReturnsFalse()
	{
		// ARRANGE
		const string marketName = "TestMarket";
		m_httpContext.Request.QueryString = new($"?market={marketName}");

		IConfiguration configuration = new ConfigurationBuilder().Build();
		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyIncludedLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenMultipleAllowedMarkets_ReturnsTrueForAllowedMarket()
	{
		// ARRANGE
		const string marketName = "TestMarket";
		m_httpContext.Request.QueryString = new($"?market={marketName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", "AllowedMarket1" },
			{ "Enabled:1", marketName },
			{ "Enabled:2", "AllowedMarket2" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyIncludedLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenMarketCaseIsDifferent_ReturnsTrue()
	{
		// ARRANGE
		const string marketName = "TestMarket";
		m_httpContext.Request.QueryString = new($"?market={marketName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", marketName.ToUpperInvariant() },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyIncludedLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenMarketIsExcluded_ReturnsFalse()
	{
		// ARRANGE
		const string marketName = "TestMarket";
		m_httpContext.Request.QueryString = new($"?market={marketName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Disabled:0", marketName },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyExcludedLogging();
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenMarketIsExcludedEvenIfInAllowedList_ReturnsFalse()
	{
		// ARRANGE
		const string marketName = "TestMarket";
		m_httpContext.Request.QueryString = new($"?market={marketName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", marketName },
			{ "Disabled:0", marketName },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyExcludedLogging();
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenMarketIsNotExcluded_ChecksAllowedMarkets()
	{
		// ARRANGE
		const string marketName = "TestMarket";
		const string excludedMarket = "ExcludedMarket";
		m_httpContext.Request.QueryString = new($"?market={marketName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", marketName },
			{ "Disabled:0", excludedMarket },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyIncludedLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenMultipleExcludedMarkets_ReturnsFalseForExcludedMarket()
	{
		// ARRANGE
		const string marketName = "TestMarket";
		m_httpContext.Request.QueryString = new($"?market={marketName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Disabled:0", "ExcludedMarket1" },
			{ "Disabled:1", marketName },
			{ "Disabled:2", "ExcludedMarket2" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyExcludedLogging();
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenExcludedMarketCaseIsDifferent_ReturnsFalse()
	{
		// ARRANGE
		const string marketName = "TestMarket";
		m_httpContext.Request.QueryString = new($"?market={marketName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Disabled:0", marketName.ToUpperInvariant() },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyExcludedLogging();
	}

	#endregion

	private void VerifyIncludedLogging(bool expectedIsEnabled) =>
		m_loggerMock.Verify(
			logger => logger.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), $"MarketFilter returning {expectedIsEnabled} for '{TestFeatureName}' as market is included.", StringComparison.Ordinal)),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);

	private void VerifyExcludedLogging() =>
		m_loggerMock.Verify(
			logger => logger.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), $"MarketFilter returning false for '{TestFeatureName}' as market is excluded.", StringComparison.Ordinal)),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
}
