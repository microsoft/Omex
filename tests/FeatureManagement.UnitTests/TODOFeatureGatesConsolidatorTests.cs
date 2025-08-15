//// Copyright (C) Microsoft Corporation. All rights reserved.

//namespace Microsoft.Omex.FeatureManagement.Tests;

//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Globalization;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
//using Microsoft.Omex.Extensions.FeatureManagement;
//using Microsoft.Omex.FeatureManagement.Constants;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;

//[TestClass]
//public sealed class FeatureGatesConsolidatorTests : IDisposable
//{
//	private readonly ActivitySource m_activitySource;
//	private readonly Mock<IFeatureGatesService> m_featureGatesServiceMock;
//	private readonly Mock<IHttpContextAccessor> m_httpContextAccessorMock;
//	private readonly Mock<ILogger<FeatureGatesConsolidator>> m_loggerMock;
//	private readonly FeatureGatesConsolidator m_consolidator;
//	private readonly HttpContext m_httpContext;
//	private readonly List<Activity> m_capturedActivities;
//	private readonly ActivityListener m_activityListener;

//	public FeatureGatesConsolidatorTests()
//	{
//		m_activitySource = new("UnitTestSource");
//		m_featureGatesServiceMock = new();
//		m_httpContextAccessorMock = new();
//		m_loggerMock = new();
//		m_httpContext = new DefaultHttpContext();
//		m_capturedActivities = new();

//		m_activityListener = new()
//		{
//			ShouldListenTo = source => source.Name == "UnitTestSource",
//			Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
//			ActivityStarted = activity => m_capturedActivities.Add(activity),
//		};
//		ActivitySource.AddActivityListener(m_activityListener);

//		m_httpContextAccessorMock.Setup(x => x.HttpContext).Returns(m_httpContext);

//		m_consolidator = new(
//			m_activitySource,
//			m_featureGatesServiceMock.Object,
//			m_httpContextAccessorMock.Object,
//			m_loggerMock.Object);
//	}

//	public void Dispose()
//	{
//		m_activityListener.Dispose();
//		m_activitySource.Dispose();
//	}

//	#region GetFeatureGatesAsync

//	[TestMethod]
//	public async Task GetFeatureGatesAsync_WhenHttpContextIsNull_ThrowsInvalidOperationException()
//	{
//		// ARRANGE
//		m_httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

//		// ACT
//		Func<Task> function = async () => await m_consolidator.GetFeatureGatesAsync();

//		// ASSERT
//		InvalidOperationException exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(function);
//		Assert.AreEqual("HttpContext is null. Ensure IHttpContextAccessor is properly configured.", exception.Message);
//	}

//	[TestMethod]
//	public async Task GetFeatureGatesAsync_WhenExperimentalFeaturesAreNull_ReturnsOnlyBasicFeatures()
//	{
//		// ARRANGE
//		SetupHttpContextWithUnauthenticatedUser();
//		Dictionary<string, object> basicFeatures = new() { { "Feature1", true }, { "Feature2", false } };
//		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
//		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<Guid>(), It.IsAny<ExperimentFilters>()))
//			.ReturnsAsync(new Dictionary<string, object>());

//		// ACT
//		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync();

//		// ASSERT
//		Assert.AreEqual(2, result.Count);
//		Assert.AreEqual(true, result["Feature1"]);
//		Assert.AreEqual(false, result["Feature2"]);
//		VerifyLoggerCalled("Successfully retrieved feature gates: Feature1=True;Feature2=False");
//	}

//	[TestMethod]
//	public async Task GetFeatureGatesAsync_WhenExperimentalFeaturesExist_MergesWithBasicFeatures()
//	{
//		// ARRANGE
//		SetupHttpContextWithAuthenticatedUser();
//		Dictionary<string, object> basicFeatures = new() { { "Feature1", true }, { "Feature2", false } };
//		Dictionary<string, object> experimentalFeatures = new() { { "Feature2", true }, { "Feature3", "experimental" } };
//		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
//		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<Guid>(), It.IsAny<ExperimentFilters>()))
//			.ReturnsAsync(experimentalFeatures);

//		// ACT
//		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync();

//		// ASSERT
//		Assert.AreEqual(3, result.Count);
//		Assert.AreEqual(true, result["Feature1"]);
//		Assert.AreEqual(true, result["Feature2"]); // Experimental overrides basic
//		Assert.AreEqual("experimental", result["Feature3"]);
//		VerifyLoggerCalled("Successfully retrieved feature gates: Feature1=True;Feature2=True;Feature3=experimental");
//	}

//	[TestMethod]
//	public async Task GetFeatureGatesAsync_WhenUserIsUnauthenticated_DoesNotRetrieveExperimentalFeatures()
//	{
//		// ARRANGE
//		SetupHttpContextWithUnauthenticatedUser();
//		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
//		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);

//		// ACT
//		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync();

//		// ASSERT
//		Assert.AreEqual(1, result.Count);
//		Assert.AreEqual(true, result["Feature1"]);
//		m_featureGatesServiceMock.Verify(x => x.GetExperimentalFeaturesAsync(It.IsAny<Guid>(), It.IsAny<ExperimentFilters>()), Times.Never);
//	}

//	[TestMethod]
//	public async Task GetFeatureGatesAsync_WhenUserIsUnauthenticated_SetsCustomerUnauthenticatedMetadata()
//	{
//		// ARRANGE
//		SetupHttpContextWithUnauthenticatedUser();
//		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
//		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);

//		// ACT
//		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync();

//		// ASSERT
//		Assert.AreEqual(1, result.Count);
//		Assert.AreEqual(1, m_capturedActivities.Count);
//		Activity activity = m_capturedActivities[0];
//		Assert.AreEqual(FeatureManagementActivityNames.FeatureGatesConsolidator.GetExperimentalFeaturesAsync, activity.OperationName);
//		string? activityResult = activity.Tags.First(tag => string.Equals(tag.Key, ActivityTagKeys.Result, StringComparison.Ordinal)).Value;
//		Assert.AreEqual("ExpectedError", activityResult);
//		string? metadata = activity.Tags.FirstOrDefault(tag => string.Equals(tag.Key, ActivityTagKeys.Metadata, StringComparison.Ordinal)).Value;
//		Assert.AreEqual("Customer unauthenticated.", metadata);
//	}

//	[TestMethod]
//	public async Task GetFeatureGatesAsync_WhenUserCannotGetEntraId_DoesNotRetrieveExperimentalFeatures()
//	{
//		// ARRANGE
//		SetupHttpContextWithAuthenticatedUserWithoutEntraId();
//		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
//		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);

//		// ACT
//		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync();

//		// ASSERT
//		Assert.AreEqual(1, result.Count);
//		Assert.AreEqual(true, result["Feature1"]);
//		m_featureGatesServiceMock.Verify(x => x.GetExperimentalFeaturesAsync(It.IsAny<Guid>(), It.IsAny<ExperimentFilters>()), Times.Never);
//	}

//	[TestMethod]
//	public async Task GetFeatureGatesAsync_WhenUserCannotGetEntraId_SetsEntraIdUnavailableMetadata()
//	{
//		// ARRANGE
//		SetupHttpContextWithAuthenticatedUserWithoutEntraId();
//		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
//		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);

//		// ACT
//		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync();

//		// ASSERT
//		Assert.AreEqual(1, result.Count);
//		Assert.AreEqual(1, m_capturedActivities.Count);
//		Activity activity = m_capturedActivities[0];
//		Assert.AreEqual(FeatureManagementActivityNames.FeatureGatesConsolidator.GetExperimentalFeaturesAsync, activity.OperationName);
//		string? activityResult = activity.Tags.First(tag => string.Equals(tag.Key, ActivityTagKeys.Result, StringComparison.Ordinal)).Value;
//		Assert.AreEqual("ExpectedError", activityResult);
//		string? metadata = activity.Tags.FirstOrDefault(tag => string.Equals(tag.Key, ActivityTagKeys.Metadata, StringComparison.Ordinal)).Value;
//		Assert.AreEqual("Entra ID unavailable.", metadata);
//	}

//	[TestMethod]
//	public async Task GetFeatureGatesAsync_WhenExperimentalFeaturesEmpty_LogsWarning()
//	{
//		// ARRANGE
//		SetupHttpContextWithAuthenticatedUser();
//		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
//		Dictionary<string, object> emptyExperimentalFeatures = new();
//		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
//		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<Guid>(), It.IsAny<ExperimentFilters>()))
//			.ReturnsAsync(emptyExperimentalFeatures);

//		// ACT
//		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync();

//		// ASSERT
//		Assert.AreEqual(1, result.Count);
//		Assert.AreEqual(1, m_capturedActivities.Count);
//		Activity activity = m_capturedActivities[0];
//		Assert.AreEqual(FeatureManagementActivityNames.FeatureGatesConsolidator.GetExperimentalFeaturesAsync, activity.OperationName);
//		string? activityResult = activity.Tags.First(tag => string.Equals(tag.Key, ActivityTagKeys.Result, StringComparison.Ordinal)).Value;
//		Assert.AreEqual("Success", activityResult);
//		string? metadata = activity.Tags.FirstOrDefault(tag => string.Equals(tag.Key, ActivityTagKeys.Metadata, StringComparison.Ordinal)).Value;
//		Assert.AreEqual("No applicable experiments.", metadata);
//	}

//	[TestMethod]
//	public async Task GetFeatureGatesAsync_WhenExperimentalFeaturesExist_LogsExperimentalFeatures()
//	{
//		// ARRANGE
//		SetupHttpContextWithAuthenticatedUser();
//		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
//		Dictionary<string, object> experimentalFeatures = new() { { "Exp1", "value1" }, { "Exp2", true } };
//		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
//		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<Guid>(), It.IsAny<ExperimentFilters>()))
//			.ReturnsAsync(experimentalFeatures);

//		// ACT
//		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync();

//		// ASSERT
//		Assert.AreEqual(3, result.Count);
//		Assert.AreEqual(1, m_capturedActivities.Count);
//		Activity activity = m_capturedActivities[0];
//		string? metadata = activity.Tags.FirstOrDefault(tag => string.Equals(tag.Key, ActivityTagKeys.Metadata, StringComparison.Ordinal)).Value;
//		Assert.AreEqual("Exp1=value1;Exp2=True", metadata);
//	}

//	[TestMethod]
//	public async Task GetFeatureGatesAsync_WithComplexRequestParameters_CreatesCorrectExperimentFilters()
//	{
//		// ARRANGE
//		SetupHttpContextWithComplexParameters();
//		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
//		Dictionary<string, object> experimentalFeatures = new() { { "Exp1", "value1" } };
//		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
//		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<Guid>(), It.IsAny<ExperimentFilters>()))
//			.ReturnsAsync(experimentalFeatures);

//		// ACT
//		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync();

//		// ASSERT
//		Assert.AreEqual(2, result.Count);
//		m_featureGatesServiceMock.Verify(x => x.GetExperimentalFeaturesAsync(
//			It.IsAny<Guid>(),
//			It.Is<ExperimentFilters>(f =>
//				f.Browser == "Chrome" &&
//				f.Campaign == "TestCampaign" &&
//				f.DeviceType == "Desktop" &&
//				f.Language.Name == "en-US" &&
//				f.Market == "US" &&
//				f.Platform == "TestPlatform")), Times.Once);
//	}

//	[TestMethod]
//	public async Task GetFeatureGatesAsync_WhenLanguageIsInvalid_UsesCultureInvariant()
//	{
//		// ARRANGE
//		SetupHttpContextWithAuthenticatedUser();
//		m_httpContext.Request.QueryString = new("?language=invalid-culture");
//		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
//		Dictionary<string, object> experimentalFeatures = new() { { "Exp1", "value1" } };
//		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
//		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<Guid>(), It.IsAny<ExperimentFilters>()))
//			.ReturnsAsync(experimentalFeatures);

//		// ACT
//		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync();

//		// ASSERT
//		Assert.AreEqual(2, result.Count);
//		m_featureGatesServiceMock.Verify(x => x.GetExperimentalFeaturesAsync(
//			It.IsAny<Guid>(),
//			It.Is<ExperimentFilters>(f => f.Language == CultureInfo.InvariantCulture)), Times.Once);
//	}

//	[TestMethod]
//	public async Task GetFeatureGatesAsync_WhenNoUserAgentHeader_UsesEmptyBrowser()
//	{
//		// ARRANGE
//		SetupHttpContextWithAuthenticatedUser();
//		m_httpContext.Request.Headers.Remove("User-Agent");
//		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
//		Dictionary<string, object> experimentalFeatures = new() { { "Exp1", "value1" } };
//		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
//		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<Guid>(), It.IsAny<ExperimentFilters>()))
//			.ReturnsAsync(experimentalFeatures);

//		// ACT
//		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync();

//		// ASSERT
//		Assert.AreEqual(2, result.Count);
//		m_featureGatesServiceMock.Verify(x => x.GetExperimentalFeaturesAsync(
//			It.IsAny<Guid>(),
//			It.Is<ExperimentFilters>(f => f.Browser == string.Empty)), Times.Once);
//	}

//	[TestMethod]
//	public async Task GetFeatureGatesAsync_WhenMobileUserAgent_SetsDeviceTypeToMobile()
//	{
//		// ARRANGE
//		SetupHttpContextWithAuthenticatedUser();
//		m_httpContext.Request.Headers["User-Agent"] = "Mozilla/5.0 (iPhone; CPU iPhone OS 14_7_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.2 Mobile/15E148 Safari/604.1";
//		Dictionary<string, object> basicFeatures = new() { { "Feature1", true } };
//		Dictionary<string, object> experimentalFeatures = new() { { "Exp1", "value1" } };
//		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(basicFeatures);
//		m_featureGatesServiceMock.Setup(x => x.GetExperimentalFeaturesAsync(It.IsAny<Guid>(), It.IsAny<ExperimentFilters>()))
//			.ReturnsAsync(experimentalFeatures);

//		// ACT
//		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync();

//		// ASSERT
//		Assert.AreEqual(2, result.Count);
//		m_featureGatesServiceMock.Verify(x => x.GetExperimentalFeaturesAsync(
//			It.IsAny<Guid>(),
//			It.Is<ExperimentFilters>(f => f.DeviceType == "Mobile")), Times.Once);
//	}

//	[TestMethod]
//	public async Task GetFeatureGatesAsync_WhenEmptyFeatureDictionary_ReturnsEmptyResult()
//	{
//		// ARRANGE
//		SetupHttpContextWithUnauthenticatedUser();
//		Dictionary<string, object> emptyFeatures = new();
//		m_featureGatesServiceMock.Setup(x => x.GetFeatureGatesAsync()).ReturnsAsync(emptyFeatures);

//		// ACT
//		IDictionary<string, object> result = await m_consolidator.GetFeatureGatesAsync();

//		// ASSERT
//		Assert.AreEqual(0, result.Count);
//		VerifyLoggerCalled("Successfully retrieved feature gates: ");
//	}

//	#endregion

//	#region Helper Methods

//	private void SetupHttpContextWithUnauthenticatedUser()
//	{
//		ClaimsPrincipal unauthenticatedUser = new();
//		m_httpContext.User = unauthenticatedUser;
//		SetupDefaultHeaders();
//	}

//	private void SetupHttpContextWithAuthenticatedUser()
//	{
//		List<Claim> claims = [
//			new(ClaimTypes.Authentication, "true"),
//			new("oid", "12345678-1234-1234-1234-123456789012"),
//		];
//		ClaimsIdentity identity = new(claims, "Bearer");
//		ClaimsPrincipal authenticatedUser = new(identity);
//		m_httpContext.User = authenticatedUser;
//		SetupDefaultHeaders();
//	}

//	private void SetupHttpContextWithAuthenticatedUserWithoutEntraId()
//	{
//		List<Claim> claims = [
//			new(ClaimTypes.Authentication, "true"),
//		];
//		ClaimsIdentity identity = new(claims, "Bearer");
//		ClaimsPrincipal authenticatedUser = new(identity);
//		m_httpContext.User = authenticatedUser;
//		SetupDefaultHeaders();
//	}

//	private void SetupHttpContextWithComplexParameters()
//	{
//		SetupHttpContextWithAuthenticatedUser();
//		m_httpContext.Request.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
//		m_httpContext.Request.QueryString = new("?campaign=TestCampaign&market=US&language=en-US&correlationId=87654321-4321-4321-4321-210987654321");
//		m_httpContext.Request.Headers["Platform"] = "TestPlatform";
//	}

//	private void SetupDefaultHeaders()
//	{
//		m_httpContext.Request.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
//		m_httpContext.Request.QueryString = new("?campaign=&market=&language=en-US&correlationId=12345678-1234-1234-1234-123456789012");
//		m_httpContext.Request.Headers["Platform"] = "MyPlatform";
//	}

//	private void VerifyLoggerCalled(string expectedMessage)
//	{
//		m_loggerMock.Verify(
//			x => x.Log(
//				LogLevel.Information,
//				It.IsAny<EventId>(),
//				It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
//				It.IsAny<Exception>(),
//				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
//			Times.Once);
//	}

//	#endregion
//}
