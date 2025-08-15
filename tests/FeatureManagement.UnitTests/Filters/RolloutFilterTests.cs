// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests.Filters;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement.Authentication;
using Microsoft.Omex.Extensions.FeatureManagement.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class RolloutFilterTests
{
	private const string TestFeatureName = "TestFeature";
	private readonly Mock<ICustomerIdProvider> m_customerIdProviderMock;
	private readonly Mock<ILogger<RolloutFilter>> m_loggerMock;
	private readonly RolloutFilter m_filter;
	private readonly FeatureFilterEvaluationContext m_context;

	public RolloutFilterTests()
	{
		m_customerIdProviderMock = new();
		m_loggerMock = new();
		m_filter = new(m_customerIdProviderMock.Object, m_loggerMock.Object);

		m_context = new()
		{
			FeatureName = TestFeatureName,
			Parameters = new ConfigurationBuilder().Build(),
		};
	}

	#region EvaluateAsync

	[TestMethod]
	public async Task EvaluateAsync_WhenCustomerIdHashIsWithinExposurePercentage_ReturnsTrue()
	{
		// ARRANGE
		string customerId = "customer123";
		int hashMod100 = Math.Abs(customerId.GetHashCode()) % 100;

		Dictionary<string, string?> configValues = new()
		{
			{ "ExposurePercentage", hashMod100.ToString(CultureInfo.InvariantCulture) },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;
		m_customerIdProviderMock.Setup(x => x.GetCustomerId()).Returns(customerId);

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenCustomerIdHashIsOutsideExposurePercentage_ReturnsFalse()
	{
		// ARRANGE
		string customerId = "customer456";
		int hashMod100 = Math.Abs(customerId.GetHashCode()) % 100;

		Dictionary<string, string?> configValues = new()
		{
			{ "ExposurePercentage", (hashMod100 - 1).ToString(CultureInfo.InvariantCulture) },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;
		m_customerIdProviderMock.Setup(x => x.GetCustomerId()).Returns(customerId);

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_When100PercentExposure_ReturnsTrue()
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "ExposurePercentage", "100" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;
		m_customerIdProviderMock.Setup(x => x.GetCustomerId()).Returns("any-customer-id");

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	[DataRow("customer1")]
	[DataRow("customer2")]
	public async Task EvaluateAsync_When0PercentExposure_ReturnsFalse(string customerId)
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "ExposurePercentage", "0" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;
		m_customerIdProviderMock.Setup(x => x.GetCustomerId()).Returns(customerId);

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
	}

	[TestMethod]
	[DataRow("customer789", 50, true)]  // Expected to hash <= 50
	[DataRow("customer999", 50, false)] // Expected to hash > 50
	public async Task EvaluateAsync_WhenExposurePercentageIs50_ReturnsExpectedResult(string customerId, int exposurePercentage, bool expectedResult)
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "ExposurePercentage", exposurePercentage.ToString(CultureInfo.InvariantCulture) },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;
		m_customerIdProviderMock.Setup(x => x.GetCustomerId()).Returns(customerId);

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		int hashMod100 = Math.Abs(customerId.GetHashCode()) % 100;
		bool expectedBasedOnHash = hashMod100 <= exposurePercentage;
		Assert.AreEqual(expectedBasedOnHash, result, $"Customer '{customerId}' with hash {hashMod100} should be {(expectedBasedOnHash ? "enabled" : "disabled")} at {exposurePercentage}% exposure");
		VerifyLogging(result);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenEmptyCustomerId_HandlesCorrectly()
	{
		// ARRANGE
		Dictionary<string, string?> configValues = new()
		{
			{ "ExposurePercentage", "50" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;
		m_customerIdProviderMock.Setup(x => x.GetCustomerId()).Returns(string.Empty);

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		// Empty string has a specific hash code. Verify it is handled correctly.
		int hashMod100 = Math.Abs(string.Empty.GetHashCode()) % 100;
		Assert.AreEqual(hashMod100 <= 50, result);
		VerifyLogging(result);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenMissingExposurePercentageConfig_UsesDefault()
	{
		// ARRANGE
		// Empty configuration (no ExposurePercentage).
		IConfiguration configuration = new ConfigurationBuilder().Build();

		m_context.Parameters = configuration;
		m_customerIdProviderMock.Setup(x => x.GetCustomerId()).Returns("customer123");

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		// Default ExposurePercentage should be 0.
		Assert.IsFalse(result);
		VerifyLogging(false);
	}

	#endregion

	private void VerifyLogging(bool expectedIsEnabled) =>
		m_loggerMock.Verify(
			logger => logger.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), $"RolloutFilter returning {expectedIsEnabled} for '{TestFeatureName}'.", StringComparison.Ordinal)),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
}
