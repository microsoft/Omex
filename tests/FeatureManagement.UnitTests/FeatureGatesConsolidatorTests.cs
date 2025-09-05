// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement.Constants;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class FeatureGatesConsolidatorTests : IDisposable
{
	public TestContext TestContext { get; set; }

	private readonly ActivitySource m_activitySource;
	private readonly Mock<IFeatureGatesService> m_featureGatesServiceMock;
	private readonly Mock<IHttpContextAccessor> m_httpContextAccessorMock;
	private readonly Mock<ILogger<FeatureGatesConsolidator>> m_loggerMock;
	private readonly FeatureGatesConsolidator m_consolidator;
	private readonly HttpContext m_httpContext;
	private readonly List<Activity> m_capturedActivities;
	private readonly ActivityListener m_activityListener;

	public FeatureGatesConsolidatorTests()
	{
		m_activitySource = new(nameof(FeatureGatesConsolidatorTests));
		m_featureGatesServiceMock = new();
		m_httpContextAccessorMock = new();
		m_loggerMock = new();
		m_httpContext = new DefaultHttpContext();
		m_capturedActivities = [];

		m_activityListener = new()
		{
			ShouldListenTo = source => source.Name == nameof(FeatureGatesConsolidatorTests),
			Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
			ActivityStarted = activity => m_capturedActivities.Add(activity),
		};
		ActivitySource.AddActivityListener(m_activityListener);

		m_httpContextAccessorMock.Setup(x => x.HttpContext).Returns(m_httpContext);

		m_consolidator = new(
			m_activitySource,
			m_featureGatesServiceMock.Object,
			m_httpContextAccessorMock.Object,
			m_loggerMock.Object);
	}

	public void Dispose()
	{
		m_activityListener.Dispose();
		m_activitySource.Dispose();
	}

	#region GetFeatureGatesAsync

	[TestMethod]
	public async Task GetFeatureGatesAsync_WhenHttpContextIsNull_ThrowsInvalidOperationException()
	{
		// ARRANGE
		m_httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
		Dictionary<string, object> filters = new() { { "test", "value" } };

		// ACT
		async Task function() => await m_consolidator.GetFeatureGatesAsync(filters, cancellationToken: TestContext.CancellationTokenSource.Token);

		// ASSERT
		InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(function);
		Assert.AreEqual("HttpContext is null. Ensure IHttpContextAccessor is properly configured.", exception.Message);
	}

	[TestMethod]
	public async Task GetFeatureGatesAsync_WithEmptyExperimentalFeatures_ReturnsOnlyBasicFeatures()
	{
		// ARRANGE
		Dictionary<string, object> filters = new() { { "filter1", "value1" } };
		Dictionary<string, object> basicFeatures = new() { { "Feature1", true }, { "Feature2", false } };
		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Dictionary<string, object>());

		// ACT
		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync(filters, cancellationToken: TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.HasCount(2, result);
		Assert.IsTrue((bool)result["Feature1"]);
		Assert.IsFalse((bool)result["Feature2"]);
		VerifyLoggerCalled(LogLevel.Information, "Successfully retrieved feature gates: 'Feature1=True;Feature2=False'.");
		VerifyLoggerCalled(LogLevel.Warning, "Experimental features returned no results");
		VerifyActivityMetadata("No applicable experiments.");
	}

	[TestMethod]
	public async Task GetFeatureGatesAsync_WithExperimentalFeatures_MergesWithBasicFeatures()
	{
		// ARRANGE
		Dictionary<string, object> filters = new() { { "filter1", "value1" } };
		Dictionary<string, object> basicFeatures = new() { { "Feature1", true }, { "Feature2", false } };
		Dictionary<string, object> experimentalFeatures = new() { { "Feature2", true }, { "Feature3", "experimental" } };
		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(experimentalFeatures);

		// ACT
		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync(filters, cancellationToken: TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.HasCount(3, result);
		Assert.IsTrue((bool)result["Feature1"]);
		Assert.IsTrue((bool)result["Feature2"]); // Experimental overrides basic.
		Assert.AreEqual("experimental", result["Feature3"]);
		VerifyLoggerCalled(LogLevel.Information, "Successfully retrieved feature gates: 'Feature1=True;Feature2=True;Feature3=experimental'.");
		VerifyLoggerCalled(LogLevel.Information, "Retrieved experimental features: 'Feature2=True;Feature3=experimental'.");
		VerifyActivityMetadata("Feature2=True;Feature3=experimental");
	}

	[TestMethod]
	public async Task GetFeatureGatesAsync_WithCancellationToken_PassesTokenToService()
	{
		// ARRANGE
		Dictionary<string, object> filters = new() { { "filter1", "value1" } };
		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
		Dictionary<string, object> experimentalFeatures = new() { { "Exp1", "value1" } };
		using CancellationTokenSource cts = new();

		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(filters, cts.Token))
			.ReturnsAsync(experimentalFeatures);

		// ACT
		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync(filters, cancellationToken: cts.Token);

		// ASSERT
		Assert.HasCount(2, result);
		m_featureGatesServiceMock.Verify(x => x.GetExperimentalFeaturesAsync(filters, cts.Token), Times.Once);
	}

	[TestMethod]
	public async Task GetFeatureGatesAsync_WithHeaderPrefixAndDefaultPlatform_SetsActivitySubType()
	{
		// ARRANGE
		Dictionary<string, object> filters = new() { { "filter1", "value1" } };
		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
		Dictionary<string, object> experimentalFeatures = new() { { "Exp1", "value1" } };
		const string headerPrefix = "X-Test-";
		const string defaultPlatform = "TestPlatform";

		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(experimentalFeatures);

		// ACT
		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync(filters, headerPrefix, defaultPlatform, cancellationToken: TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.HasCount(2, result);
		Assert.HasCount(1, m_capturedActivities);
		Activity activity = m_capturedActivities[0];
		Assert.AreEqual(FeatureManagementActivityNames.FeatureGatesConsolidator.GetExperimentalFeaturesAsync, activity.OperationName);
	}

	[TestMethod]
	public async Task GetFeatureGatesAsync_WhenCalled_LogsFiltersCorrectly()
	{
		// ARRANGE
		Dictionary<string, object> filters = new()
		{
			{ "userId", "user123" },
			{ "market", "US" },
			{ "version", 2 },
		};
		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
		Dictionary<string, object> experimentalFeatures = new() { { "Exp1", "value1" } };

		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(experimentalFeatures);

		// ACT
		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync(filters, cancellationToken: TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.HasCount(2, result);
		VerifyLoggerCalled(LogLevel.Information, "Requesting experimental features with filters:");
	}

	[TestMethod]
	public async Task GetFeatureGatesAsync_WhenExperimentalFeatureOverridesBasicFeature_ExperimentalTakesPrecedence()
	{
		// ARRANGE
		Dictionary<string, object> filters = new() { { "filter1", "value1" } };
		Dictionary<string, object> basicFeatures = new()
		{
			{ "Feature1", false },
			{ "Feature2", "basic" },
			{ "Feature3", 123 },
		};
		Dictionary<string, object> experimentalFeatures = new()
		{
			{ "Feature1", true },           // Override Boolean
			{ "Feature2", "experimental" }, // Override string
			{ "Feature3", 456 },            // Override number
		};

		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(experimentalFeatures);

		// ACT
		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync(filters, cancellationToken: TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.HasCount(3, result);
		Assert.IsTrue((bool)result["Feature1"]);
		Assert.AreEqual("experimental", result["Feature2"]);
		Assert.AreEqual(456, result["Feature3"]);
	}

	[TestMethod]
	public async Task GetFeatureGatesAsync_WithEmptyFilters_StillCallsExperimentalFeatures()
	{
		// ARRANGE
		Dictionary<string, object> filters = new();
		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
		Dictionary<string, object> experimentalFeatures = new() { { "Exp1", "value1" } };

		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(experimentalFeatures);

		// ACT
		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync(filters, cancellationToken: TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.HasCount(2, result);
		m_featureGatesServiceMock.Verify(x => x.GetExperimentalFeaturesAsync(filters, It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task GetFeatureGatesAsync_WhenCalled_ActivityIsMarkedAsSuccess()
	{
		// ARRANGE
		Dictionary<string, object> filters = new() { { "filter1", "value1" } };
		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
		Dictionary<string, object> experimentalFeatures = new() { { "Exp1", "value1" } };

		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(experimentalFeatures);

		// ACT
		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync(filters, cancellationToken: TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.HasCount(1, m_capturedActivities);
		Activity activity = m_capturedActivities[0];
		string? activityResult = activity.Tags.FirstOrDefault(tag => string.Equals(tag.Key, ActivityTagKeys.Result, StringComparison.Ordinal)).Value;
		Assert.AreEqual("Success", activityResult);
	}

	[TestMethod]
	public async Task GetFeatureGatesAsync_WithLargeNumberOfFeatures_HandlesCorrectly()
	{
		// ARRANGE
		Dictionary<string, object> filters = new() { { "filter1", "value1" } };
		Dictionary<string, object> basicFeatures = new();
		Dictionary<string, object> experimentalFeatures = new();

		// Add many features.
		for (int i = 0; i < 100; i++)
		{
			basicFeatures[$"BasicFeature{i}"] = i % 2 == 0;
			experimentalFeatures[$"ExpFeature{i}"] = $"value{i}";
		}

		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(experimentalFeatures);

		// ACT
		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync(filters, cancellationToken: TestContext.CancellationTokenSource.Token);

		// ASSERT
		Assert.HasCount(200, result);
	}

	#endregion

	private void VerifyLoggerCalled(LogLevel logLevel, string expectedMessage) =>
		m_loggerMock.Verify(
			x => x.Log(
				logLevel,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.AtLeastOnce);

	private void VerifyActivityMetadata(string expectedMetadata)
	{
		Assert.IsGreaterThan(0, m_capturedActivities.Count, "No activities were captured.");
		Activity activity = m_capturedActivities.Last();
		string? metadata = activity.Tags.FirstOrDefault(tag => string.Equals(tag.Key, ActivityTagKeys.Metadata, StringComparison.Ordinal)).Value;
		Assert.AreEqual(expectedMetadata, metadata);
	}
}
