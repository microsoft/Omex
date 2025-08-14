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
public sealed class CampaignFilterTests
{
	private const string TestFeatureName = "TestFeature";
	private readonly Mock<IHttpContextAccessor> m_httpContextAccessorMock;
	private readonly Mock<ILogger<CampaignFilter>> m_loggerMock;
	private readonly CampaignFilter m_filter;
	private readonly FeatureFilterEvaluationContext m_context;
	private readonly HttpContext m_httpContext;

	public CampaignFilterTests()
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
	public async Task EvaluateAsync_WhenCampaignIsAllowed_ReturnsTrue()
	{
		// ARRANGE
		const string campaignName = "TestCampaign";
		m_httpContext.Request.QueryString = new($"?campaign={campaignName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", campaignName },
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
	public async Task EvaluateAsync_WhenCampaignIsNotAllowed_ReturnsFalse()
	{
		// ARRANGE
		const string campaignName = "TestCampaign";
		const string allowedCampaign = "AllowedCampaign";
		m_httpContext.Request.QueryString = new($"?campaign={campaignName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", allowedCampaign },
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
	public async Task EvaluateAsync_WhenNoCampaignInQuery_ReturnsFalse()
	{
		// ARRANGE
		const string allowedCampaign = "AllowedCampaign";
		m_httpContext.Request.QueryString = new(string.Empty);

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", allowedCampaign },
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
			{ "Enabled:0", "AllowedCampaign" },
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
	public async Task EvaluateAsync_WhenNoAllowedCampaigns_ReturnsFalse()
	{
		// ARRANGE
		const string campaignName = "TestCampaign";
		m_httpContext.Request.QueryString = new($"?campaign={campaignName}");

		IConfiguration configuration = new ConfigurationBuilder().Build();
		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyIncludedLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenMultipleAllowedCampaigns_ReturnsTrueForAllowedCampaign()
	{
		// ARRANGE
		const string campaignName = "TestCampaign";
		m_httpContext.Request.QueryString = new($"?campaign={campaignName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", "AllowedCampaign1" },
			{ "Enabled:1", campaignName },
			{ "Enabled:2", "AllowedCampaign2" },
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
	public async Task EvaluateAsync_WhenCampaignCaseIsDifferent_ReturnsTrue()
	{
		// ARRANGE
		const string campaignName = "TestCampaign";
		m_httpContext.Request.QueryString = new($"?campaign={campaignName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", campaignName.ToUpperInvariant() },
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
	public async Task EvaluateAsync_WhenCampaignIsExcluded_ReturnsFalse()
	{
		// ARRANGE
		const string campaignName = "TestCampaign";
		m_httpContext.Request.QueryString = new($"?campaign={campaignName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Disabled:0", campaignName },
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
	public async Task EvaluateAsync_WhenCampaignIsExcludedEvenIfInAllowedList_ReturnsFalse()
	{
		// ARRANGE
		const string campaignName = "TestCampaign";
		m_httpContext.Request.QueryString = new($"?campaign={campaignName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", campaignName },
			{ "Disabled:0", campaignName },
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
	public async Task EvaluateAsync_WhenCampaignIsNotExcluded_ChecksAllowedCampaigns()
	{
		// ARRANGE
		const string campaignName = "TestCampaign";
		const string excludedCampaign = "ExcludedCampaign";
		m_httpContext.Request.QueryString = new($"?campaign={campaignName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Enabled:0", campaignName },
			{ "Disabled:0", excludedCampaign },
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
	public async Task EvaluateAsync_WhenMultipleExcludedCampaigns_ReturnsFalseForExcludedCampaign()
	{
		// ARRANGE
		const string campaignName = "TestCampaign";
		m_httpContext.Request.QueryString = new($"?campaign={campaignName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Disabled:0", "ExcludedCampaign1" },
			{ "Disabled:1", campaignName },
			{ "Disabled:2", "ExcludedCampaign2" },
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
	public async Task EvaluateAsync_WhenExcludedCampaignCaseIsDifferent_ReturnsFalse()
	{
		// ARRANGE
		const string campaignName = "TestCampaign";
		m_httpContext.Request.QueryString = new($"?campaign={campaignName}");

		Dictionary<string, string?> configValues = new()
		{
			{ "Disabled:0", campaignName.ToUpperInvariant() },
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
				It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), $"CampaignFilter returning {expectedIsEnabled} for '{TestFeatureName}' as campaign is included.", StringComparison.Ordinal)),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);

	private void VerifyExcludedLogging() =>
		m_loggerMock.Verify(
			logger => logger.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), $"CampaignFilter returning false for '{TestFeatureName}' as campaign is excluded.", StringComparison.Ordinal)),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
}
