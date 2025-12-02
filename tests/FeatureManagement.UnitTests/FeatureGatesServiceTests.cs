// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement.Experimentation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class FeatureGatesServiceTests
{
	public TestContext TestContext { get; set; }

	private readonly ActivitySource m_activitySourceMock;
	private readonly Mock<IExperimentManager> m_experimentManagerMock;
	private readonly Mock<IExtendedFeatureManager> m_featureManagerMock;
	private readonly Mock<ILogger<FeatureGatesService>> m_loggerMock;
	private readonly FeatureGatesService m_featureGatesService;

	public FeatureGatesServiceTests()
	{
		m_activitySourceMock = new(nameof(FeatureGatesServiceTests));
		m_experimentManagerMock = new();
		m_featureManagerMock = new();
		m_loggerMock = new();

		m_featureGatesService = new(
			m_activitySourceMock,
			m_experimentManagerMock.Object,
			m_featureManagerMock.Object,
			m_loggerMock.Object);
	}

	#region RequestedFeatures

	[TestMethod]
	public void RequestedFeatures_WhenCalled_ReturnsFromFeatureManager()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.EnabledFeatures).Returns("FE_Test1;FE_Test2");

		// ACT
		string result = m_featureGatesService.RequestedFeatures;

		// ASSERT
		Assert.AreEqual("FE_Test1;FE_Test2", result);
	}

	#endregion

	#region BlockedFeatures

	[TestMethod]
	public void BlockedFeatures_WhenCalled_ReturnsFromFeatureManager()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.DisabledFeatures).Returns("FE_Blocked1");

		// ACT
		string result = m_featureGatesService.BlockedFeatures;

		// ASSERT
		Assert.AreEqual("FE_Blocked1", result);
	}

	#endregion

	#region GetFeatureGatesAsync

	[TestMethod]
	public async Task GetFeatureGatesAsync_WhenFeaturesPresent_ReturnsExpectedDictionary()
	{
		// ARRANGE
		List<string> features = ["FE_Test1", "FE_Test2", "Other"];
		m_featureManagerMock.Setup(m => m.GetFeatureNamesAsync()).Returns(features.ToAsyncEnumerable());
		m_featureManagerMock.Setup(m => m.IsEnabledAsync("FE_Test1")).ReturnsAsync(true);
		m_featureManagerMock.Setup(m => m.IsEnabledAsync("FE_Test2")).ReturnsAsync(false);
		m_featureManagerMock.Setup(m => m.EnabledFeaturesList).Returns(["FE_Test2"]);
		m_featureManagerMock.Setup(m => m.DisabledFeaturesList).Returns(["FE_Test1"]);

		// ACT
		IDictionary<string, object> result = await m_featureGatesService.GetFeatureGatesAsync();

		// ASSERT
		Assert.HasCount(2, result);
		Assert.IsFalse((bool)result["Test1"]); // DisabledFeaturesList overrides to false.
		Assert.IsTrue((bool)result["Test2"]);  // EnabledFeaturesList overrides to true.
	}

	[TestMethod]
	public async Task GetFeatureGatesAsync_WhenNoFrontendFeatures_ReturnsEmptyDictionary()
	{
		// ARRANGE
		List<string> features = ["Other1", "Other2"];
		m_featureManagerMock.Setup(m => m.GetFeatureNamesAsync()).Returns(features.ToAsyncEnumerable());
		m_featureManagerMock.Setup(m => m.EnabledFeaturesList).Returns([]);
		m_featureManagerMock.Setup(m => m.DisabledFeaturesList).Returns([]);

		// ACT
		IDictionary<string, object> result = await m_featureGatesService.GetFeatureGatesAsync();

		// ASSERT
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task GetFeatureGatesAsync_WhenFeatureAlreadyExistsInMap_DoesNotOverwrite()
	{
		// ARRANGE
		List<string> features = ["FE_Test1", "fe_test1"]; // Different casing
		m_featureManagerMock.Setup(m => m.GetFeatureNamesAsync()).Returns(features.ToAsyncEnumerable());
		m_featureManagerMock.Setup(m => m.IsEnabledAsync("FE_Test1")).ReturnsAsync(true);
		m_featureManagerMock.Setup(m => m.IsEnabledAsync("fe_test1")).ReturnsAsync(false);
		m_featureManagerMock.Setup(m => m.EnabledFeaturesList).Returns([]);
		m_featureManagerMock.Setup(m => m.DisabledFeaturesList).Returns([]);

		// ACT
		IDictionary<string, object> result = await m_featureGatesService.GetFeatureGatesAsync();

		// ASSERT
		Assert.HasCount(1, result);
		Assert.IsTrue((bool)result["Test1"]); // First value wins due to TryAdd.
	}

	[TestMethod]
	public async Task GetFeatureGatesAsync_WithEnabledFeaturesListWithoutPrefix_HandlesCorrectly()
	{
		// ARRANGE
		List<string> features = [];
		m_featureManagerMock.Setup(m => m.GetFeatureNamesAsync()).Returns(features.ToAsyncEnumerable());
		m_featureManagerMock.Setup(m => m.EnabledFeaturesList).Returns(["Test1", "FE_Test2"]);
		m_featureManagerMock.Setup(m => m.DisabledFeaturesList).Returns([]);

		// ACT
		IDictionary<string, object> result = await m_featureGatesService.GetFeatureGatesAsync();

		// ASSERT
		Assert.HasCount(2, result);
		Assert.IsTrue((bool)result["Test1"]);
		Assert.IsTrue((bool)result["Test2"]);
	}

	[TestMethod]
	public async Task GetFeatureGatesAsync_WithDisabledFeaturesListWithoutPrefix_HandlesCorrectly()
	{
		// ARRANGE
		List<string> features = [];
		m_featureManagerMock.Setup(m => m.GetFeatureNamesAsync()).Returns(features.ToAsyncEnumerable());
		m_featureManagerMock.Setup(m => m.EnabledFeaturesList).Returns([]);
		m_featureManagerMock.Setup(m => m.DisabledFeaturesList).Returns(["Test1", "FE_Test2"]);

		// ACT
		IDictionary<string, object> result = await m_featureGatesService.GetFeatureGatesAsync();

		// ASSERT
		Assert.HasCount(2, result);
		Assert.IsFalse((bool)result["Test1"]);
		Assert.IsFalse((bool)result["Test2"]);
	}

	#endregion

	#region GetExperimentalFeaturesAsync

	[TestMethod]
	public async Task GetExperimentalFeaturesAsync_WhenCalled_ReturnsFeatureFlags()
	{
		// ARRANGE
		Dictionary<string, object> expectedResponse = new(StringComparer.Ordinal)
		{
			{ "Gate1", true },
			{ "Gate2", "value" },
		};
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResponse);

		Dictionary<string, string> filters = new()
		{
			{ "CustomerId", "12345" },
			{ "Market", "US" },
		};

		// ACT
		IDictionary<string, object> result = await m_featureGatesService.GetExperimentalFeaturesAsync(filters, TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.HasCount(2, result);
		Assert.IsTrue((bool)result["Gate1"]);
		Assert.AreEqual("value", result["Gate2"]);
	}

	[TestMethod]
	public async Task GetExperimentalFeaturesAsync_WhenCalled_LogsFiltersCorrectly()
	{
		// ARRANGE
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Dictionary<string, object>());

		Dictionary<string, string> filters = new()
		{
			{ "CustomerId", "12345" },
			{ "Market", "US" },
		};

		// ACT
		await m_featureGatesService.GetExperimentalFeaturesAsync(filters, TestContext.CancellationTokenSource.Token);

		// ASSERT
		m_loggerMock.Verify(
			x => x.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CustomerId:12345") && v.ToString()!.Contains("Market:US")),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
	}

	#endregion

	#region GetExperimentFeatureValueAsync

	[TestMethod]
	[DataRow("")]
	[DataRow("   ")]
	public async Task GetExperimentFeatureValueAsync_WhenFeatureGateIsEmptyOrWhitespace_ThrowsArgumentException(string featureGate) =>
		// ACT & ASSERT
		await Assert.ThrowsExactlyAsync<ArgumentException>(
			() => m_featureGatesService.GetExperimentFeatureValueAsync(featureGate, new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token));

	[TestMethod]
	public async Task GetExperimentFeatureValueAsync_WhenFeatureNotPresent_ReturnsFalse()
	{
		// ARRANGE
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Dictionary<string, object>());

		// ACT
		FeatureGateResult result = await m_featureGatesService.GetExperimentFeatureValueAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsFalse(result.InTreatment);
		Assert.IsNull(result.Value);
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("   ")]
	public async Task GetExperimentFeatureValueAsync_WhenFeatureValueIsEmptyOrWhiteSpace_ReturnsFalse(string featureValue)
	{
		// ARRANGE
		Dictionary<string, object> featureFlags = new() { { "Gate1", featureValue } };
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(featureFlags);

		// ACT
		FeatureGateResult result = await m_featureGatesService.GetExperimentFeatureValueAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsFalse(result.InTreatment);
		Assert.IsNull(result.Value);
	}

	[TestMethod]
	public async Task GetExperimentFeatureValueAsync_WhenFeatureValueIsTrue_ReturnsTrue()
	{
		// ARRANGE
		Dictionary<string, object> featureFlags = new() { { "Gate1", true } };
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(featureFlags);

		// ACT
		FeatureGateResult result = await m_featureGatesService.GetExperimentFeatureValueAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsTrue(result.InTreatment);
		Assert.IsNull(result.Value);
	}

	[TestMethod]
	public async Task GetExperimentFeatureValueAsync_WhenFeatureValueIsFalse_ReturnsFalse()
	{
		// ARRANGE
		Dictionary<string, object> featureFlags = new() { { "Gate1", false } };
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(featureFlags);

		// ACT
		FeatureGateResult result = await m_featureGatesService.GetExperimentFeatureValueAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsFalse(result.InTreatment);
		Assert.IsNull(result.Value);
	}

	[TestMethod]
	public async Task GetExperimentFeatureValueAsync_WhenFeatureValueIsTrueString_ReturnsTrue()
	{
		// ARRANGE
		Dictionary<string, object> featureFlags = new() { { "Gate1", "true" } };
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(featureFlags);

		// ACT
		FeatureGateResult result = await m_featureGatesService.GetExperimentFeatureValueAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsTrue(result.InTreatment);
		Assert.IsNull(result.Value);
	}

	[TestMethod]
	public async Task GetExperimentFeatureValueAsync_WhenFeatureValueIsFalseString_ReturnsFalse()
	{
		// ARRANGE
		Dictionary<string, object> featureFlags = new() { { "Gate1", "false" } };
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(featureFlags);

		// ACT
		FeatureGateResult result = await m_featureGatesService.GetExperimentFeatureValueAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsFalse(result.InTreatment);
		Assert.IsNull(result.Value);
	}

	[TestMethod]
	public async Task GetExperimentFeatureValueAsync_WhenFeatureValueIsNonBoolString_ReturnsTrueWithValue()
	{
		// ARRANGE
		Dictionary<string, object> featureFlags = new() { { "Gate1", "custom" } };
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(featureFlags);

		// ACT
		FeatureGateResult result = await m_featureGatesService.GetExperimentFeatureValueAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsTrue(result.InTreatment);
		Assert.AreEqual("custom", result.Value);
	}

	#endregion

	#region IsFeatureGateApplicableAsync

	[TestMethod]
	[DataRow("")]
	[DataRow("   ")]
	public async Task IsFeatureGateApplicableAsync_WhenFeatureGateIsEmptyOrWhiteSpace_ThrowsArgumentException(string featureGate) =>
		// ACT & ASSERT
		await Assert.ThrowsExactlyAsync<ArgumentException>(
			() => m_featureGatesService.IsFeatureGateApplicableAsync(featureGate));

	[TestMethod]
	public async Task IsFeatureGateApplicableAsync_WhenFeatureEnabled_ReturnsTrue()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.IsEnabledAsync("FE_Test1")).ReturnsAsync(true);

		// ACT
		bool result = await m_featureGatesService.IsFeatureGateApplicableAsync("FE_Test1");

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task IsFeatureGateApplicableAsync_WhenFeatureDisabled_ReturnsFalse()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.IsEnabledAsync("FE_Test1")).ReturnsAsync(false);

		// ACT
		bool result = await m_featureGatesService.IsFeatureGateApplicableAsync("FE_Test1");

		// ASSERT
		Assert.IsFalse(result);
	}

	#endregion

	#region IsExperimentApplicableAsync

	[TestMethod]
	[DataRow("")]
	[DataRow("   ")]
	public async Task IsExperimentApplicableAsync_WhenFeatureGateIsEmpty_ThrowsArgumentException(string featureGate) =>
		// ACT & ASSERT
		await Assert.ThrowsExactlyAsync<ArgumentException>(
			() => m_featureGatesService.IsExperimentApplicableAsync(featureGate, new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token));

	[TestMethod]
	public async Task IsExperimentApplicableAsync_WhenOverrideExistsTrue_ReturnsTrue()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns(true);

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsTrue(result);
		m_experimentManagerMock.Verify(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task IsExperimentApplicableAsync_WhenOverrideExistsFalse_ReturnsFalse()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns(false);

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsFalse(result);
		m_experimentManagerMock.Verify(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task IsExperimentApplicableAsync_WhenNoOverrideAndFeatureNotInExperiment_ChecksFeatureManager()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns((bool?)null);
		m_featureManagerMock.Setup(m => m.IsEnabledAsync("Gate1")).ReturnsAsync(true);
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Dictionary<string, object>());

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task IsExperimentApplicableAsync_WhenNoOverrideAndFeatureValueIsEmpty_ChecksFeatureManager()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns((bool?)null);
		m_featureManagerMock.Setup(m => m.IsEnabledAsync("Gate1")).ReturnsAsync(true);
		Dictionary<string, object> featureFlags = new() { { "Gate1", string.Empty } };
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(featureFlags);

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task IsExperimentApplicableAsync_WhenExperimentValueIsTrue_ReturnsTrue()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns((bool?)null);
		Dictionary<string, object> featureFlags = new() { { "Gate1", "true" } };
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(featureFlags);

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task IsExperimentApplicableAsync_WhenExperimentValueIsFalse_ReturnsFalse()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns((bool?)null);
		Dictionary<string, object> featureFlags = new() { { "Gate1", "false" } };
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(featureFlags);

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task IsExperimentApplicableAsync_WhenExperimentValueIsTrueBool_ReturnsTrue()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns((bool?)null);
		Dictionary<string, object> featureFlags = new() { { "Gate1", true } };
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(featureFlags);

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task IsExperimentApplicableAsync_WhenExperimentValueIsFalseBool_ReturnsFalse()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns((bool?)null);
		Dictionary<string, object> featureFlags = new() { { "Gate1", false } };
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(featureFlags);

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task IsExperimentApplicableAsync_WhenExperimentValueIsNonBool_ReturnsTrue()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns((bool?)null);
		Dictionary<string, object> featureFlags = new() { { "Gate1", "custom" } };
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(featureFlags);

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task IsExperimentApplicableAsync_WhenNoOverrideAndFeatureValueIsWhitespaceAndFeatureDisabled_ReturnsFalse()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns((bool?)null);
		m_featureManagerMock.Setup(m => m.IsEnabledAsync("Gate1")).ReturnsAsync(false);
		Dictionary<string, object> featureFlags = new() { { "Gate1", "   " } };
		m_experimentManagerMock.Setup(e => e.GetFlightsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(featureFlags);

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", new Dictionary<string, string>(), TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.IsFalse(result);
		m_featureManagerMock.Verify(m => m.IsEnabledAsync("Gate1"), Times.Once);
	}

	#endregion
}
