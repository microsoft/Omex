// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.FeatureManagement.Tests.Filters;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class ToggleFilterTests
{
	private const string TestFeatureName = "TestFeature";
	private readonly Mock<IHttpContextAccessor> m_httpContextAccessorMock;
	private readonly Mock<ILogger<ToggleFilter>> m_loggerMock;
	private readonly Mock<IOptionsMonitor<FeatureOverrideSettings>> m_settingsMock;
	private readonly ToggleFilter m_filter;
	private readonly FeatureFilterEvaluationContext m_context;
	private readonly FeatureOverrideSettings m_featureOverrideSettings;
	private readonly DefaultHttpContext m_httpContext;

	public ToggleFilterTests()
	{
		m_httpContextAccessorMock = new();
		m_loggerMock = new();
		m_settingsMock = new();
		m_featureOverrideSettings = new();
		m_httpContext = new();

		m_settingsMock.Setup(s => s.CurrentValue).Returns(m_featureOverrideSettings);
		m_httpContextAccessorMock.Setup(h => h.HttpContext).Returns(m_httpContext);

		m_filter = new(m_httpContextAccessorMock.Object, m_loggerMock.Object, m_settingsMock.Object);
		m_context = new()
		{
			FeatureName = TestFeatureName,
		};
	}

	#region EvaluateAsync

	[TestMethod]
	public async Task EvaluateAsync_WhenFeatureIsToggledInQueryString_ReturnsTrue()
	{
		// ARRANGE
		m_httpContext.Request.QueryString = new($"?toggledFeatures={TestFeatureName}");

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenFeatureIsToggledInQueryStringWithDifferentCase_ReturnsTrue()
	{
		// ARRANGE
		m_httpContext.Request.QueryString = new($"?toggledFeatures={TestFeatureName.ToUpperInvariant()}");

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenFeatureIsToggledInSettings_ReturnsTrue()
	{
		// ARRANGE
		m_featureOverrideSettings.Toggled = [TestFeatureName];

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenFeatureIsToggledInSettingsWithDifferentCase_ReturnsTrue()
	{
		// ARRANGE
		m_featureOverrideSettings.Toggled = [TestFeatureName.ToUpperInvariant()];

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenNoToggleSourcesAreActive_ReturnsFalse()
	{
		// ARRANGE
		// Default setup with no toggled features

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
		m_httpContextAccessorMock.Setup(h => h.HttpContext).Returns((HttpContext?)null);

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenMultipleFeaturesToggledInQueryString_FindsCorrectFeature()
	{
		// ARRANGE
		m_httpContext.Request.QueryString = new($"?toggledFeatures=OtherFeature;{TestFeatureName};AnotherFeature");

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenMultipleFeaturesInSettingsArray_FindsCorrectFeature()
	{
		// ARRANGE
		m_featureOverrideSettings.Toggled = ["OtherFeature", TestFeatureName, "AnotherFeature"];

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenFeatureInBothQueryStringAndSettings_ReturnsTrue()
	{
		// ARRANGE
		m_httpContext.Request.QueryString = new($"?toggledFeatures={TestFeatureName}");
		m_featureOverrideSettings.Toggled = [TestFeatureName];

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenFeatureNotInQueryStringButInSettings_ReturnsTrue()
	{
		// ARRANGE
		m_httpContext.Request.QueryString = new("?toggledFeatures=OtherFeature");
		m_featureOverrideSettings.Toggled = [TestFeatureName];

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenFeatureInQueryStringButNotInSettings_ReturnsTrue()
	{
		// ARRANGE
		m_httpContext.Request.QueryString = new($"?toggledFeatures={TestFeatureName}");
		m_featureOverrideSettings.Toggled = ["OtherFeature"];

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenEmptyQueryStringParameter_ReturnsFalse()
	{
		// ARRANGE
		m_httpContext.Request.QueryString = new("?toggledFeatures=");

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenEmptySettingsArray_ReturnsFalse()
	{
		// ARRANGE
		m_featureOverrideSettings.Toggled = [];

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenQueryStringHasMultipleSeparators_ParsesCorrectly()
	{
		// ARRANGE
		m_httpContext.Request.QueryString = new($"?toggledFeatures=;;{TestFeatureName};;OtherFeature;;");

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	#endregion

	private void VerifyLogging(bool expectedIsEnabled) =>
		m_loggerMock.Verify(
			logger => logger.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), $"ToggleFilter returning {expectedIsEnabled} for '{TestFeatureName}'.", StringComparison.Ordinal)),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
}
