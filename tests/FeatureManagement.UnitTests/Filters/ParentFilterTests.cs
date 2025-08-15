// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.FeatureManagement.Tests.Filters;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class ParentFilterTests
{
	private const string TestFeatureName = "TestFeature";
	private const string ParentFeatureName = "ParentFeature";
	private readonly Mock<ILogger<ParentFilter>> m_loggerMock;
	private readonly Mock<IServiceProvider> m_serviceProviderMock;
	private readonly Mock<IExtendedFeatureManager> m_extendedFeatureManagerMock;
	private readonly ParentFilter m_filter;
	private readonly FeatureFilterEvaluationContext m_context;

	public ParentFilterTests()
	{
		m_loggerMock = new();
		m_serviceProviderMock = new();
		m_extendedFeatureManagerMock = new();
		m_filter = new(m_loggerMock.Object, m_serviceProviderMock.Object);
		m_context = new()
		{
			FeatureName = TestFeatureName,
			Parameters = new ConfigurationBuilder().Build(),
		};
	}

	#region EvaluateAsync

	[TestMethod]
	public async Task EvaluateAsync_WhenExtendedFeatureManagerCannotBeResolved_ReturnsFalse()
	{
		// ARRANGE
		m_serviceProviderMock.Setup(sp => sp.GetService(typeof(IExtendedFeatureManager)))
			.Returns((object?)null);

		Dictionary<string, string?> configValues = new()
		{
			{ "Feature", ParentFeatureName },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyErrorLogging();
		m_extendedFeatureManagerMock.Verify(m => m.IsEnabledAsync(It.IsAny<string>()), Times.Never);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenFeatureNameIsNull_ReturnsFalse()
	{
		// ARRANGE
		m_serviceProviderMock.Setup(sp => sp.GetService(typeof(IExtendedFeatureManager)))
			.Returns(m_extendedFeatureManagerMock.Object);

		Dictionary<string, string?> configValues = new()
		{
			{ "Feature", (string?)null },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		m_extendedFeatureManagerMock.Verify(m => m.IsEnabledAsync(It.IsAny<string>()), Times.Never);
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("   ")]
	[DataRow("\t")]
	[DataRow("\n")]
	public async Task EvaluateAsync_WhenFeatureNameIsEmptyOrWhiteSpace_ReturnsFalse(string featureNameValue)
	{
		// ARRANGE
		m_serviceProviderMock.Setup(sp => sp.GetService(typeof(IExtendedFeatureManager)))
			.Returns(m_extendedFeatureManagerMock.Object);

		Dictionary<string, string?> configValues = new()
		{
			{ "Feature", featureNameValue },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		m_extendedFeatureManagerMock.Verify(m => m.IsEnabledAsync(It.IsAny<string>()), Times.Never);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenParentFeatureIsEnabled_ReturnsTrue()
	{
		// ARRANGE
		m_serviceProviderMock.Setup(sp => sp.GetService(typeof(IExtendedFeatureManager)))
			.Returns(m_extendedFeatureManagerMock.Object);

		Dictionary<string, string?> configValues = new()
		{
			{ "Feature", ParentFeatureName },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		m_extendedFeatureManagerMock.Setup(m => m.IsEnabledAsync(ParentFeatureName))
			.ReturnsAsync(true);

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		m_extendedFeatureManagerMock.Verify(m => m.IsEnabledAsync(ParentFeatureName), Times.Once);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenParentFeatureIsDisabled_ReturnsFalse()
	{
		// ARRANGE
		m_serviceProviderMock.Setup(sp => sp.GetService(typeof(IExtendedFeatureManager)))
			.Returns(m_extendedFeatureManagerMock.Object);

		Dictionary<string, string?> configValues = new()
		{
			{ "Feature", ParentFeatureName },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		m_extendedFeatureManagerMock.Setup(m => m.IsEnabledAsync(ParentFeatureName))
			.ReturnsAsync(false);

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		m_extendedFeatureManagerMock.Verify(m => m.IsEnabledAsync(ParentFeatureName), Times.Once);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenEmptyConfiguration_ReturnsFalse()
	{
		// ARRANGE
		m_serviceProviderMock.Setup(sp => sp.GetService(typeof(IExtendedFeatureManager)))
			.Returns(m_extendedFeatureManagerMock.Object);

		IConfiguration configuration = new ConfigurationBuilder().Build();
		m_context.Parameters = configuration;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		m_extendedFeatureManagerMock.Verify(m => m.IsEnabledAsync(It.IsAny<string>()), Times.Never);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenParentFeatureThrowsException_ExceptionPropagates()
	{
		// ARRANGE
		m_serviceProviderMock.Setup(sp => sp.GetService(typeof(IExtendedFeatureManager)))
			.Returns(m_extendedFeatureManagerMock.Object);

		Dictionary<string, string?> configValues = new()
		{
			{ "Feature", ParentFeatureName },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		m_extendedFeatureManagerMock.Setup(m => m.IsEnabledAsync(ParentFeatureName))
			.ThrowsAsync(new InvalidOperationException("Test exception"));

		// ACT
		Func<Task> function = async () => await m_filter.EvaluateAsync(m_context);

		// ASSERT
		InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(function);
		Assert.AreEqual("Test exception", exception.Message);
		m_extendedFeatureManagerMock.Verify(m => m.IsEnabledAsync(ParentFeatureName), Times.Once);
	}

	#endregion

	private void VerifyErrorLogging() =>
		m_loggerMock.Verify(
			logger => logger.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ParentFilter could not resolve the IExtendedFeatureManager object")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
}
