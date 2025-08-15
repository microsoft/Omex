// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.FeatureManagement.Tests.Filters;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement.Filters;
using Microsoft.Omex.Extensions.FeatureManagement.Filters.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class EnvironmentFilterTests
{
	private const string TestFeatureName = "TestFeature";
	private readonly Mock<IOptionsMonitor<ClusterSettings>> m_settingsMock;
	private readonly Mock<ILogger<EnvironmentFilter>> m_loggerMock;
	private readonly EnvironmentFilter m_filter;
	private readonly FeatureFilterEvaluationContext m_context;
	private readonly ClusterSettings m_clusterSettings;

	public EnvironmentFilterTests()
	{
		m_settingsMock = new();
		m_loggerMock = new();
		m_clusterSettings = new() { Environment = "Production" };
		m_settingsMock.Setup(s => s.CurrentValue).Returns(m_clusterSettings);
		m_filter = new(m_settingsMock.Object, m_loggerMock.Object);
		m_context = new()
		{
			FeatureName = TestFeatureName,
			Parameters = new ConfigurationBuilder().Build(),
		};
	}

	#region EvaluateAsync

	[TestMethod]
	public async Task EvaluateAsync_WhenEnvironmentIsAllowed_ReturnsTrue()
	{
		// ARRANGE
		const string environment = "Production";
		m_clusterSettings.Environment = environment;

		Dictionary<string, string?> configValues = new()
		{
			{ "Environments:0", environment },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenEnvironmentIsNotAllowed_ReturnsFalse()
	{
		// ARRANGE
		const string environment = "Production";
		const string allowedEnvironment = "Development";
		m_clusterSettings.Environment = environment;

		Dictionary<string, string?> configValues = new()
		{
			{ "Environments:0", allowedEnvironment },
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
	public async Task EvaluateAsync_WhenMultipleEnvironmentsAllowed_ReturnsTrue()
	{
		// ARRANGE
		const string environment = "Production";
		m_clusterSettings.Environment = environment;

		Dictionary<string, string?> configValues = new()
		{
			{ "Environments:0", "Development" },
			{ "Environments:1", "Staging" },
			{ "Environments:2", environment },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenNoEnvironmentsSpecified_ReturnsFalse()
	{
		// ARRANGE
		const string environment = "Production";
		m_clusterSettings.Environment = environment;

		IConfiguration configuration = new ConfigurationBuilder().Build();
		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenEnvironmentCaseIsDifferent_ReturnsTrue()
	{
		// ARRANGE
		const string environment = "Production";
		m_clusterSettings.Environment = environment;

		Dictionary<string, string?> configValues = new()
		{
			{ "Environments:0", "production" }, // Lowercase
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

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

				It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), $"EnvironmentFilter returning {expectedIsEnabled} for '{TestFeatureName}'.", StringComparison.Ordinal)),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
}
