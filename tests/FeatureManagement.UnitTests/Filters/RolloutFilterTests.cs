// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.FeatureManagement.Tests.Filters;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Omex.FeatureManagement.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class RolloutFilterTests
{
	private const string EntraIdType = "oid";
	private const string TestFeatureName = "TestFeature";
	private readonly Mock<IHttpContextAccessor> m_httpContextAccessorMock;
	private readonly Mock<ILogger<RolloutFilter>> m_loggerMock;
	private readonly RolloutFilter m_filter;
	private readonly FeatureFilterEvaluationContext m_context;
	private readonly HttpContext m_httpContext;
	private readonly ClaimsPrincipal m_claimsPrincipal;

	public RolloutFilterTests()
	{
		m_httpContextAccessorMock = new();
		m_loggerMock = new();
		m_filter = new(m_httpContextAccessorMock.Object, m_loggerMock.Object);

		m_httpContext = new DefaultHttpContext();
		m_claimsPrincipal = new();
		m_httpContext.User = m_claimsPrincipal;
		m_httpContextAccessorMock.Setup(x => x.HttpContext).Returns(m_httpContext);

		m_context = new()
		{
			FeatureName = TestFeatureName,
			Parameters = new ConfigurationBuilder().Build(),
		};
	}

	#region EvaluateAsync

	[TestMethod]
	public async Task EvaluateAsync_WhenEntraIdIsNotAvailable_ReturnsFalse()
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

		// Make GetEntraId return default Guid
		Mock<ClaimsPrincipal> claimsPrincipalMock = new();
		m_httpContext.User = claimsPrincipalMock.Object;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
		VerifyEntraIdNotFoundLogging();
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenEntraIdHashIsWithinExposurePercentage_ReturnsTrue()
	{
		// ARRANGE
		// Create a known Guid and find out its hash code % 100.
		Guid testGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
		int hashMod100 = Math.Abs(testGuid.GetHashCode() % 100);

		Dictionary<string, string?> configValues = new()
		{
			{ "ExposurePercentage", hashMod100.ToString(CultureInfo.InvariantCulture) },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// Setup a ClaimsPrincipal with the proper claim for Entra ID, instead of trying to mock the extension method directly (which is not supported).
		ClaimsPrincipal principal = new();
		principal.AddIdentity(new(
		[
			new(EntraIdType, testGuid.ToString()),
		]));
		m_httpContext.User = principal;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	public async Task EvaluateAsync_WhenEntraIdHashIsOutsideExposurePercentage_ReturnsFalse()
	{
		// ARRANGE
		// Create a known Guid and find out its hash code % 100.
		Guid testGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
		int hashMod100 = Math.Abs(testGuid.GetHashCode() % 100);

		Dictionary<string, string?> configValues = new()
		{
			{ "ExposurePercentage", (hashMod100 - 1).ToString(CultureInfo.InvariantCulture) },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// Setup a ClaimsPrincipal with the proper claim for Entra ID, instead of trying to mock the extension method directly (which is not supported).
		ClaimsPrincipal principal = new();
		principal.AddIdentity(new(
		[
			new(EntraIdType, testGuid.ToString()),
		]));
		m_httpContext.User = principal;

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

		// Setup GetEntraId to return a non-default Guid.
		Guid testGuid = Guid.NewGuid();

		// Setup a ClaimsPrincipal with the proper claim for Entra ID, instead of trying to mock the extension method directly (which is not supported).
		ClaimsPrincipal principal = new();
		principal.AddIdentity(new(
		[
			new(EntraIdType, testGuid.ToString()),
		]));
		m_httpContext.User = principal;

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsTrue(result);
		VerifyLogging(true);
	}

	[TestMethod]
	[DataRow("12d07e1a-c87f-40a6-bf24-052f5b66f131")]
	[DataRow("e29f7a59-5f33-4b4d-ac0c-49b447e9bfe1")] // This GUID has a negative hash code, which tests the absolute value logic.
	public async Task EvaluateAsync_When0PercentExposure_ReturnsFalse(string entraId)
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

		// Setup a ClaimsPrincipal with the proper claim for Entra ID, instead of trying to mock the extension method directly (which is not supported).
		ClaimsPrincipal principal = new();
		principal.AddIdentity(new(
		[
			new(EntraIdType, entraId),
		]));
		m_httpContext.User = principal;

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
		Dictionary<string, string?> configValues = new()
		{
			{ "ExposurePercentage", "100" },
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configValues)
			.Build();

		m_context.Parameters = configuration;

		// Setup null HttpContext
		m_httpContextAccessorMock.Setup(h => h.HttpContext).Returns((HttpContext?)null);

		// ACT
		bool result = await m_filter.EvaluateAsync(m_context);

		// ASSERT
		Assert.IsFalse(result);
		VerifyLogging(false);
		VerifyEntraIdNotFoundLogging();
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

	private void VerifyEntraIdNotFoundLogging() =>
		m_loggerMock.Verify(
			logger => logger.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), $"RolloutFilter could not fetch the Entra ID.", StringComparison.Ordinal)),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
}
