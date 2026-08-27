// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests.Filters;

using System;
using System.Threading.Tasks;
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
	private readonly Mock<ILogger<ToggleFilter>> m_loggerMock;
	private readonly Mock<IOptionsMonitor<FeatureOverrideSettings>> m_settingsMock;
	private readonly ToggleFilter m_filter;
	private readonly FeatureFilterEvaluationContext m_context;
	private readonly FeatureOverrideSettings m_featureOverrideSettings;

	public ToggleFilterTests()
	{
		m_loggerMock = new();
		m_settingsMock = new();
		m_featureOverrideSettings = new();

		m_settingsMock.Setup(s => s.CurrentValue).Returns(m_featureOverrideSettings);

		m_filter = new(m_loggerMock.Object, m_settingsMock.Object);
		m_context = new()
		{
			FeatureName = TestFeatureName,
		};
	}

	#region EvaluateAsync

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
		// Default setup with no toggled features.

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
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

	#endregion

	private void VerifyLogging(bool expectedIsEnabled) =>
		m_loggerMock.Verify(
			logger => logger.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), $"ToggleFilter returning '{expectedIsEnabled}' for '{TestFeatureName}'.", StringComparison.Ordinal)),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
}
