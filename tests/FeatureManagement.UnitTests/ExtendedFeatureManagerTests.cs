// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.FeatureManagement.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.FeatureManagement;
using Microsoft.Omex.FeatureManagement.Constants;
using Microsoft.Omex.FeatureManagement.Tests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class ExtendedFeatureManagerTests
{
	private readonly Mock<IFeatureManager> m_featureManagerMock;
	private HttpContextAccessor m_httpContextAccessorMock;
	private readonly Mock<ILogger<ExtendedFeatureManager>> m_loggerMock;
	private readonly Mock<IOptionsMonitor<FeatureOverrideSettings>> m_settingsMock;
	private readonly ExtendedFeatureManager m_extendedFeatureManager;

	public ExtendedFeatureManagerTests()
	{
		m_featureManagerMock = new();
		m_httpContextAccessorMock = HttpContextAccessorFactory.Create();
		m_loggerMock = new();
		m_settingsMock = new();

		FeatureOverrideSettings settings = new()
		{
			Disabled = ["DisabledFeature1", "DisabledFeature2"],
			Enabled = ["EnabledFeature1", "EnabledFeature2"],
		};
		m_settingsMock.Setup(s => s.CurrentValue).Returns(settings);

		m_extendedFeatureManager = new(
			m_featureManagerMock.Object,
			m_httpContextAccessorMock,
			m_loggerMock.Object,
			m_settingsMock.Object);
	}

	#region DisabledFeatures

	[TestMethod]
	public void DisabledFeatures_WhenCalled_ReturnsValueFromHttpContextAccessor()
	{
		// ARRANGE
		StringValues expectedValue = new("feature1;feature2");
		InitializeHttpContextAccessor(RequestParameters.Query.DisabledFeatures, expectedValue);

		// ACT
		string result = m_extendedFeatureManager.DisabledFeatures;

		// ASSERT
		Assert.AreEqual(expectedValue.ToString(), result);
	}

	#endregion

	#region DisabledFeaturesList

	[TestMethod]
	public void DisabledFeaturesList_WithNoFeatures_ReturnsEmptyArray()
	{
		// ARRANGE
		StringValues expectedValue = new();
		InitializeHttpContextAccessor(RequestParameters.Query.DisabledFeatures, expectedValue);

		// ACT
		string[] result = m_extendedFeatureManager.DisabledFeaturesList;

		// ASSERT
		Assert.AreEqual(0, result.Length);
	}

	[TestMethod]
	public void DisabledFeaturesList_WithMultipleFeatures_SplitsStringCorrectly()
	{
		// ARRANGE
		StringValues expectedValue = new("feature1;feature2;feature3");
		InitializeHttpContextAccessor(RequestParameters.Query.DisabledFeatures, expectedValue);

		// ACT
		string[] result = m_extendedFeatureManager.DisabledFeaturesList;

		// ASSERT
		CollectionAssert.AreEqual(new[] { "feature1", "feature2", "feature3" }, result);
	}

	[TestMethod]
	public void DisabledFeaturesList_WithEmptyFeatures_SplitsStringCorrectly()
	{
		// ARRANGE
		StringValues expectedValue = new("feature1;;feature2;;feature3;");
		InitializeHttpContextAccessor(RequestParameters.Query.DisabledFeatures, expectedValue);

		// ACT
		string[] result = m_extendedFeatureManager.DisabledFeaturesList;

		// ASSERT
		CollectionAssert.AreEqual(new[] { "feature1", "feature2", "feature3" }, result);
	}

	#endregion

	#region EnabledFeatures

	[TestMethod]
	public void EnabledFeatures_WhenCalled_ReturnsValueFromHttpContextAccessor()
	{
		// ARRANGE
		StringValues expectedValue = new("feature1;feature2");
		InitializeHttpContextAccessor(RequestParameters.Query.EnabledFeatures, expectedValue);

		// ACT
		string result = m_extendedFeatureManager.EnabledFeatures;

		// ASSERT
		Assert.AreEqual(expectedValue.ToString(), result);
	}

	#endregion

	#region EnabledFeaturesList

	[TestMethod]
	public void EnabledFeaturesList_WithNoFeatures_ReturnsEmptyArray()
	{
		// ARRANGE
		StringValues expectedValue = new();
		InitializeHttpContextAccessor(RequestParameters.Query.EnabledFeatures, expectedValue);

		// ACT
		string[] result = m_extendedFeatureManager.EnabledFeaturesList;

		// ASSERT
		Assert.AreEqual(0, result.Length);
	}

	[TestMethod]
	public void EnabledFeaturesList_WithMultipleFeatures_SplitsStringCorrectly()
	{
		// ARRANGE
		StringValues expectedValue = new("feature1;feature2;feature3");
		InitializeHttpContextAccessor(RequestParameters.Query.EnabledFeatures, expectedValue);

		// ACT
		string[] result = m_extendedFeatureManager.EnabledFeaturesList;

		// ASSERT
		CollectionAssert.AreEqual(new[] { "feature1", "feature2", "feature3" }, result);
	}

	[TestMethod]
	public void EnabledFeaturesList_WithEmptyFeatures_SplitsStringCorrectly()
	{
		// ARRANGE
		StringValues expectedValue = new("feature1;;feature2;;feature3;");
		InitializeHttpContextAccessor(RequestParameters.Query.EnabledFeatures, expectedValue);

		// ACT
		string[] result = m_extendedFeatureManager.EnabledFeaturesList;

		// ASSERT
		CollectionAssert.AreEqual(new[] { "feature1", "feature2", "feature3" }, result);
	}

	#endregion

	#region GetOverride

	[TestMethod]
	[DataRow("")]
	[DataRow(" ")]
	public void GetOverride_WithEmptyOrWhitespaceFeatureName_ThrowsArgumentException(string featureName)
	{
		// ARRANGE
		Func<bool?> action = () => m_extendedFeatureManager.GetOverride(featureName);

		// ASSERT
		ArgumentException exception = Assert.ThrowsException<ArgumentException>(() => action());
		Assert.Contains("feature", exception.Message);
	}

	[TestMethod]
	[DataRow("requestedFeature")]
	[DataRow("REQUESTEDFEATURE")] // Case-insensitive
	public void GetOverride_WhenFeatureInEnabledFeatures_ReturnsTrueWhen(string featureName)
	{
		// ARRANGE
		const string requestedFeature = "requestedFeature";
		InitializeHttpContextAccessor(RequestParameters.Query.EnabledFeatures, requestedFeature);

		// ACT
		bool? result = m_extendedFeatureManager.GetOverride(featureName);

		// ASSERT
		Assert.IsTrue(result.HasValue);
		Assert.IsTrue(result.Value);
	}

	[TestMethod]
	[DataRow("EnabledFeature1")]
	[DataRow("enabledfeature2")] // Case-insensitive
	public void GetOverride_WhenFeatureInOverrideEnabled_ReturnsTrue(string featureName)
	{
		// ARRANGE
		// Uses settings initialized in constructor.

		// ACT
		bool? result = m_extendedFeatureManager.GetOverride(featureName);

		// ASSERT
		Assert.IsTrue(result.HasValue);
		Assert.IsTrue(result.Value);
	}

	[TestMethod]
	[DataRow("blockedFeature")]
	[DataRow("BLOCKEDFEATURE")] // Case-insensitive
	public void GetOverride_WhenFeatureInDisabledFeatures_ReturnsFalse(string featureName)
	{
		// ARRANGE
		const string blockedFeature = "blockedFeature";
		InitializeHttpContextAccessor(RequestParameters.Query.DisabledFeatures, blockedFeature);

		// ACT
		bool? result = m_extendedFeatureManager.GetOverride(featureName);

		// ASSERT
		Assert.IsTrue(result.HasValue);
		Assert.IsFalse(result.Value);
	}

	[TestMethod]
	[DataRow("DisabledFeature1")]
	[DataRow("disabledfeature2")] // Case-insensitive
	public void GetOverride_WhenFeatureInOverrideDisabled_ReturnsFalse(string featureName)
	{
		// ARRANGE
		// Uses settings initialized in constructor.

		// ACT
		bool? result = m_extendedFeatureManager.GetOverride(featureName);

		// ASSERT
		Assert.IsTrue(result.HasValue);
		Assert.IsFalse(result.Value);
	}

	[TestMethod]
	public void GetOverride_WhenFeatureNotInAnyOverride_ReturnsNull()
	{
		// ARRANGE
		const string featureName = "NoOverrideFeature";

		// ACT
		bool? result = m_extendedFeatureManager.GetOverride(featureName);

		// ASSERT
		Assert.IsNull(result);
	}

	#endregion

	#region GetFeatureNamesAsync

	[TestMethod]
	public async Task GetFeatureNamesAsync_WhenCalled_DelegatesCallToFeatureManager()
	{
		// ARRANGE
		IAsyncEnumerable<string> expectedNames = new[] { "Feature1", "Feature2" }.ToAsyncEnumerable();
		m_featureManagerMock.Setup(m => m.GetFeatureNamesAsync()).Returns(expectedNames);

		// ACT
		IAsyncEnumerable<string> result = m_extendedFeatureManager.GetFeatureNamesAsync();
		List<string> resultList = await result.ToListAsync();

		// ASSERT
		m_featureManagerMock.Verify(m => m.GetFeatureNamesAsync(), Times.Once);
		CollectionAssert.AreEqual(await expectedNames.ToListAsync(), resultList);
	}

	#endregion

	#region IsEnabledAsync

	[TestMethod]
	[DataRow("")]
	[DataRow(" ")]
	public async Task IsEnabledAsync_WithEmptyOrWhitespaceFeatureName_ThrowsArgumentException(string featureName)
	{
		// ARRANGE
		Func<Task> action = async () => await m_extendedFeatureManager.IsEnabledAsync(featureName);

		// ASSERT
		ArgumentException exception = await Assert.ThrowsExceptionAsync<ArgumentException>(action);
		Assert.Contains("feature", exception.Message);
	}

	[TestMethod]
	[DataRow(true)]
	[DataRow(false)]
	public async Task IsEnabledAsync_WhenNoOverrideExists_DelegatesCallToFeatureManager(bool featureManagerResult)
	{
		// ARRANGE
		const string featureName = "TestFeature";
		m_featureManagerMock.Setup(m => m.IsEnabledAsync(featureName))
			.ReturnsAsync(featureManagerResult);

		// ACT
		bool result = await m_extendedFeatureManager.IsEnabledAsync(featureName);

		// ASSERT
		Assert.AreEqual(featureManagerResult, result);
		m_featureManagerMock.Verify(m => m.IsEnabledAsync(featureName), Times.Once);
	}

	#endregion

	#region GetFeatureNamesAsync<TContext>

	[TestMethod]
	[DataRow("")]
	[DataRow(" ")]
	public async Task IsEnabledAsyncWithContext_WithEmptyOrWhitespaceFeatureName_ThrowsArgumentException(string featureName)
	{
		// ARRANGE
		const int context = 0;
		Func<Task> action = async () => await m_extendedFeatureManager.IsEnabledAsync(featureName, context);

		// ASSERT
		ArgumentException exception = await Assert.ThrowsExceptionAsync<ArgumentException>(action);
		Assert.Contains("feature", exception.Message);
	}

	[TestMethod]
	[DataRow(true)]
	[DataRow(false)]
	public async Task IsEnabledAsyncWithContext_WhenCalled_DelegatesCallToFeatureManager(bool featureManagerResult)
	{
		// ARRANGE
		const string featureName = "TestFeature";
		const int context = 0;
		m_featureManagerMock.Setup(m => m.IsEnabledAsync(featureName, context))
			.ReturnsAsync(featureManagerResult);

		// ACT
		bool result = await m_extendedFeatureManager.IsEnabledAsync(featureName, context);

		// ASSERT
		Assert.AreEqual(featureManagerResult, result);
		m_featureManagerMock.Verify(m => m.IsEnabledAsync(featureName, context), Times.Once);
	}

	#endregion

	private void InitializeHttpContextAccessor(string paramName, StringValues paramValue)
	{
		Dictionary<string, StringValues> queryParameters = new(StringComparer.Ordinal)
		{
			{ paramName, paramValue },
		};

		m_httpContextAccessorMock = HttpContextAccessorFactory.Create(queryParameters);
	}
}
