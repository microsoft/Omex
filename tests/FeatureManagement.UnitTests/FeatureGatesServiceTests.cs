// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.FeatureManagement.Tests;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.OMEX.Experimentation.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class FeatureGatesServiceTests
{
	private readonly ActivitySource m_activitySourceMock;
	private readonly Mock<IExperimentationService> m_experimentationServiceMock;
	private readonly Mock<IExtendedFeatureManager> m_featureManagerMock;
	private readonly Mock<ILogger<FeatureGatesService>> m_loggerMock;
	private readonly FeatureGatesService m_featureGatesService;

	public FeatureGatesServiceTests()
	{
		m_activitySourceMock = new(nameof(FeatureGatesServiceTests));
		m_experimentationServiceMock = new();
		m_featureManagerMock = new();
		m_loggerMock = new();

		m_featureGatesService = new(
			m_activitySourceMock,
			m_experimentationServiceMock.Object,
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
		Assert.AreEqual(2, result.Count);
		Assert.AreEqual(false, result["Test1"]); // BlockedFeaturesList overrides to false.
		Assert.AreEqual(true, result["Test2"]);  // RequestedFeaturesList overrides to true.
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
		Assert.AreEqual(0, result.Count);
	}

	#endregion

	#region GetExperimentalFeaturesAsync

	[TestMethod]
	public async Task GetExperimentalFeaturesAsync_WhenCalled_ReturnsFeatureFlags()
	{
		// ARRANGE
		Dictionary<string, object> featureFlags = new(StringComparer.Ordinal)
		{
			{ "Gate1", true },
			{ "Gate2", "value" }
		};
		m_experimentationServiceMock.Setup(e => e.GetFlightsAsync(It.IsAny<AllocateRequest>()))
			.ReturnsAsync(new AllocateResponse { FeatureFlags = featureFlags });

		ExperimentFilters filters = GetDefaultFilters();

		// ACT
		IDictionary<string, object> result = await m_featureGatesService.GetExperimentalFeaturesAsync(Guid.NewGuid(), filters);

		// ASSERT
		Assert.AreEqual(2, result.Count);
		Assert.AreEqual(true, result["Gate1"]);
		Assert.AreEqual("value", result["Gate2"]);
	}

	#endregion

	#region GetExperimentFeatureValueAsync

	[TestMethod]
	public async Task GetExperimentFeatureValueAsync_WhenFeatureNotPresent_ReturnsFalse()
	{
		// ARRANGE
		m_experimentationServiceMock.Setup(e => e.GetFlightsAsync(It.IsAny<AllocateRequest>()))
			.ReturnsAsync(new AllocateResponse { FeatureFlags = new Dictionary<string, object>() });

		ExperimentFilters filters = GetDefaultFilters();

		// ACT
		FeatureGateResult result = await m_featureGatesService.GetExperimentFeatureValueAsync("Gate1", Guid.NewGuid(), filters);

		// ASSERT
		Assert.IsFalse(result.InTreatment);
		Assert.IsNull(result.Value);
	}

	[TestMethod]
	public async Task GetExperimentFeatureValueAsync_WhenFeatureValueIsEmpty_ReturnsFalse()
	{
		// ARRANGE
		Dictionary<string, object> featureFlags = new() { { "Gate1", string.Empty } };
		m_experimentationServiceMock.Setup(e => e.GetFlightsAsync(It.IsAny<AllocateRequest>()))
			.ReturnsAsync(new AllocateResponse { FeatureFlags = featureFlags });

		ExperimentFilters filters = GetDefaultFilters();

		// ACT
		FeatureGateResult result = await m_featureGatesService.GetExperimentFeatureValueAsync("Gate1", Guid.NewGuid(), filters);

		// ASSERT
		Assert.IsFalse(result.InTreatment);
		Assert.IsNull(result.Value);
	}

	[TestMethod]
	public async Task GetExperimentFeatureValueAsync_WhenFeatureValueIsBool_ReturnsBool()
	{
		// ARRANGE
		Dictionary<string, object> featureFlags = new() { { "Gate1", true } };
		m_experimentationServiceMock.Setup(e => e.GetFlightsAsync(It.IsAny<AllocateRequest>()))
			.ReturnsAsync(new AllocateResponse { FeatureFlags = featureFlags });

		ExperimentFilters filters = GetDefaultFilters();

		// ACT
		FeatureGateResult result = await m_featureGatesService.GetExperimentFeatureValueAsync("Gate1", Guid.NewGuid(), filters);

		// ASSERT
		Assert.IsTrue(result.InTreatment);
		Assert.IsNull(result.Value);
	}

	[TestMethod]
	public async Task GetExperimentFeatureValueAsync_WhenFeatureValueIsString_ReturnsTrueWithValue()
	{
		// ARRANGE
		Dictionary<string, object> featureFlags = new() { { "Gate1", "custom" } };
		m_experimentationServiceMock.Setup(e => e.GetFlightsAsync(It.IsAny<AllocateRequest>()))
			.ReturnsAsync(new AllocateResponse { FeatureFlags = featureFlags });

		ExperimentFilters filters = GetDefaultFilters();

		// ACT
		FeatureGateResult result = await m_featureGatesService.GetExperimentFeatureValueAsync("Gate1", Guid.NewGuid(), filters);

		// ASSERT
		Assert.IsTrue(result.InTreatment);
		Assert.AreEqual("custom", result.Value);
	}

	#endregion

	#region IsFeatureGateApplicableAsync

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
	public async Task IsExperimentApplicableAsync_WhenOverrideExists_ReturnsOverride()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns(true);

		ExperimentFilters filters = GetDefaultFilters();

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", Guid.NewGuid(), filters);

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task IsExperimentApplicableAsync_WhenNoExperimentValueAndFeatureEnabled_ReturnsTrue()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns((bool?)null);
		m_featureManagerMock.Setup(m => m.IsEnabledAsync("Gate1")).ReturnsAsync(true);
		m_experimentationServiceMock.Setup(e => e.GetFlightsAsync(It.IsAny<AllocateRequest>()))
			.ReturnsAsync(new AllocateResponse { FeatureFlags = new Dictionary<string, object>(StringComparer.Ordinal) });

		ExperimentFilters filters = GetDefaultFilters();

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", Guid.NewGuid(), filters);

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task IsExperimentApplicableAsync_WhenNoExperimentValueAndFeatureDisabled_ReturnsFalse()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns((bool?)null);
		m_featureManagerMock.Setup(m => m.IsEnabledAsync("Gate1")).ReturnsAsync(false);
		m_experimentationServiceMock.Setup(e => e.GetFlightsAsync(It.IsAny<AllocateRequest>()))
			.ReturnsAsync(new AllocateResponse { FeatureFlags = new Dictionary<string, object>(StringComparer.Ordinal) });

		ExperimentFilters filters = GetDefaultFilters();

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", Guid.NewGuid(), filters);

		// ASSERT
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task IsExperimentApplicableAsync_WhenExperimentValueIsBool_ReturnsParsedValue()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns((bool?)null);
		Dictionary<string, object> featureFlags = new(StringComparer.Ordinal) { { "Gate1", "true" } };
		m_experimentationServiceMock.Setup(e => e.GetFlightsAsync(It.IsAny<AllocateRequest>()))
			.ReturnsAsync(new AllocateResponse { FeatureFlags = featureFlags });

		ExperimentFilters filters = GetDefaultFilters();

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", Guid.NewGuid(), filters);

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task IsExperimentApplicableAsync_WhenExperimentValueIsNonBool_ReturnsTrue()
	{
		// ARRANGE
		m_featureManagerMock.Setup(m => m.GetOverride("Gate1")).Returns((bool?)null);
		Dictionary<string, object> featureFlags = new(StringComparer.Ordinal) { { "Gate1", "custom" } };
		m_experimentationServiceMock.Setup(e => e.GetFlightsAsync(It.IsAny<AllocateRequest>()))
			.ReturnsAsync(new AllocateResponse { FeatureFlags = featureFlags });

		ExperimentFilters filters = GetDefaultFilters();

		// ACT
		bool result = await m_featureGatesService.IsExperimentApplicableAsync("Gate1", Guid.NewGuid(), filters);

		// ASSERT
		Assert.IsTrue(result);
	}

	#endregion

	#region GetExperimentalFeaturesAsync

	[TestMethod]
	public async Task GetExperimentalFeaturesAsync_WhenEntraIdIsEmpty_Throws()
	{
		// ARRANGE
		ExperimentFilters filters = GetDefaultFilters();

		// ACT
		Func<Task> function = async () => await m_featureGatesService.GetExperimentalFeaturesAsync(Guid.Empty, filters);

		// ASSERT
		ArgumentException exception = await Assert.ThrowsExceptionAsync<ArgumentException>(function);
		Assert.Contains("entraId", exception.Message);
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("   ")]
	public async Task GetExperimentFeatureValueAsync_WhenFeatureGateOrEntraIdInvalid_Throws(string value)
	{
		// ARRANGE
		ExperimentFilters filters = GetDefaultFilters();

		// ACT
		Func<Task> function1 = async () => await m_featureGatesService.GetExperimentFeatureValueAsync(value, Guid.NewGuid(), filters);
		Func<Task> function2 = async () => await m_featureGatesService.GetExperimentFeatureValueAsync("Gate1", Guid.Empty, filters);

		// ASSERT
		ArgumentException exception1 = await Assert.ThrowsExceptionAsync<ArgumentException>(function1);
		ArgumentException exception2 = await Assert.ThrowsExceptionAsync<ArgumentException>(function2);
		Assert.Contains("featureGate", exception1.Message);
		Assert.Contains("entraId", exception2.Message);
	}

	#endregion

	#region IsFeatureGateApplicableAsync

	[TestMethod]
	[DataRow("")]
	[DataRow("   ")]
	public async Task IsFeatureGateApplicableAsync_WhenFeatureGateInvalid_Throws(string featureGate)
	{
		// ACT
		Func<Task> function = async () => await m_featureGatesService.IsFeatureGateApplicableAsync(featureGate);

		// ASSERT
		ArgumentException exception = await Assert.ThrowsExceptionAsync<ArgumentException>(function);
		Assert.Contains("featureGate", exception.Message);
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("   ")]
	public async Task IsExperimentApplicableAsync_WhenFeatureGateOrEntraIdInvalid_Throws(string value)
	{
		// ARRANGE
		ExperimentFilters filters = GetDefaultFilters();

		// ACT
		Func<Task> function1 = async () => await m_featureGatesService.IsExperimentApplicableAsync(value, Guid.NewGuid(), filters);
		Func<Task> function2 = async () => await m_featureGatesService.IsExperimentApplicableAsync("Gate1", Guid.Empty, filters);

		// ASSERT
		ArgumentException exception1 = await Assert.ThrowsExceptionAsync<ArgumentException>(function1);
		ArgumentException exception2 = await Assert.ThrowsExceptionAsync<ArgumentException>(function2);
		Assert.Contains("featureGate", exception1.Message);
		Assert.Contains("entraId", exception2.Message);
	}

	#endregion

	private static ExperimentFilters GetDefaultFilters() =>
		new()
		{
			Browser = "Edge",
			Campaign = "TestCampaign",
			CorrelationId = Guid.NewGuid(),
			DeviceType = "Desktop",
			Language = CultureInfo.InvariantCulture,
			Market = "US",
			Platform = "MyPlatform",
		};
}
