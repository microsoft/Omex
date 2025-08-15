// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests.Authentication;

using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.FeatureManagement.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class EntraIdProviderTests
{
	private const string EntraIdClaimType = "oid";
	private readonly Mock<IHttpContextAccessor> m_httpContextAccessorMock;
	private readonly Mock<ILogger<EntraIdProvider>> m_loggerMock;
	private readonly EntraIdProvider m_provider;

	public EntraIdProviderTests()
	{
		m_httpContextAccessorMock = new();
		m_loggerMock = new();
		m_provider = new(m_httpContextAccessorMock.Object, m_loggerMock.Object);
	}

	#region GetCustomerId

	[TestMethod]
	public void GetCustomerId_WhenHttpContextIsNull_ReturnsEmptyStringAndLogsInformation()
	{
		// ARRANGE
		m_httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

		// ACT
		string result = m_provider.GetCustomerId();

		// ASSERT
		Assert.AreEqual(string.Empty, result);
		VerifyInformationLogged();
	}

	[TestMethod]
	public void GetCustomerId_WhenUserHasNoEntraId_ReturnsEmptyStringAndLogsInformation()
	{
		// ARRANGE
		HttpContext httpContext = new DefaultHttpContext();
		ClaimsPrincipal principal = new();
		httpContext.User = principal;
		m_httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

		// ACT
		string result = m_provider.GetCustomerId();

		// ASSERT
		Assert.AreEqual(string.Empty, result);
		VerifyInformationLogged();
	}

	[TestMethod]
	public void GetCustomerId_WhenUserHasEntraIdClaim_ReturnsEntraIdString()
	{
		// ARRANGE
		Guid expectedEntraId = Guid.NewGuid();
		HttpContext httpContext = new DefaultHttpContext();
		ClaimsIdentity identity = new(
		[
			new Claim(EntraIdClaimType, expectedEntraId.ToString())
		]);
		ClaimsPrincipal principal = new(identity);
		httpContext.User = principal;
		m_httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

		// ACT
		string result = m_provider.GetCustomerId();

		// ASSERT
		Assert.AreEqual(expectedEntraId.ToString(), result);
		VerifyNoLogging();
	}

	[TestMethod]
	public void GetCustomerId_WhenUserHasInvalidEntraIdClaim_ReturnsEmptyStringAndLogsInformation()
	{
		// ARRANGE
		HttpContext httpContext = new DefaultHttpContext();
		ClaimsIdentity identity = new(
		[
			new(EntraIdClaimType, "not-a-valid-guid"),
		]);
		ClaimsPrincipal principal = new(identity);
		httpContext.User = principal;
		m_httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

		// ACT
		string result = m_provider.GetCustomerId();

		// ASSERT
		Assert.AreEqual(string.Empty, result);
		VerifyInformationLogged();
	}

	[TestMethod]
	public void GetCustomerId_WhenUserHasMultipleClaims_ReturnsCorrectEntraId()
	{
		// ARRANGE
		Guid expectedEntraId = Guid.NewGuid();
		HttpContext httpContext = new DefaultHttpContext();
		ClaimsIdentity identity = new(
		[
			new("name", "Test User"),
			new("email", "test@example.com"),
			new(EntraIdClaimType, expectedEntraId.ToString()),
			new("role", "admin"),
		]);
		ClaimsPrincipal principal = new(identity);
		httpContext.User = principal;
		m_httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

		// ACT
		string result = m_provider.GetCustomerId();

		// ASSERT
		Assert.AreEqual(expectedEntraId.ToString(), result);
		VerifyNoLogging();
	}

	[TestMethod]
	public void GetCustomerId_WhenCalledMultipleTimes_ReturnsConsistentResults()
	{
		// ARRANGE
		Guid expectedEntraId = Guid.NewGuid();
		HttpContext httpContext = new DefaultHttpContext();
		ClaimsIdentity identity = new(
		[
			new(EntraIdClaimType, expectedEntraId.ToString()),
		]);
		ClaimsPrincipal principal = new(identity);
		httpContext.User = principal;
		m_httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

		// ACT
		string result1 = m_provider.GetCustomerId();
		string result2 = m_provider.GetCustomerId();
		string result3 = m_provider.GetCustomerId();

		// ASSERT
		Assert.AreEqual(expectedEntraId.ToString(), result1);
		Assert.AreEqual(expectedEntraId.ToString(), result2);
		Assert.AreEqual(expectedEntraId.ToString(), result3);
		VerifyNoLogging();
	}

	#endregion

	private void VerifyInformationLogged() =>
		m_loggerMock.Verify(
			logger => logger.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => string.Equals(v.ToString(), "EntraIdProvider could not fetch the Entra ID.", StringComparison.Ordinal)),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);

	private void VerifyNoLogging() =>
		m_loggerMock.Verify(
			logger => logger.Log(
				It.IsAny<LogLevel>(),
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Never);
}
