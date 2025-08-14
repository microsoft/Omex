// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.FeatureManagement.Tests.Extensions;

using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Omex.FeatureManagement.Constants;
using Microsoft.Omex.FeatureManagement.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class HttpContextAccessorExtensionsTests
{
	#region GetParameter

	[TestMethod]
	[DataRow("")]
	[DataRow(" ")]
	public void GetParameter_WhenParameterNameIsEmptyOrWhitespace_ThrowsArgumentException(string paramName)
	{
		// ARRANGE
		Mock<IHttpContextAccessor> mockAccessor = new();

		// ACT
		Func<string> action = () => mockAccessor.Object.GetParameter(paramName);

		// ASSERT
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => action());
		Assert.Contains("paramName", exception.Message);
	}

	[TestMethod]
	public void GetParameter_WhenHttpContextIsNull_ReturnsEmptyStringValues()
	{
		// ARRANGE
		const string paramName = "TestParam";
		Mock<IHttpContextAccessor> mockAccessor = new();
		mockAccessor.Setup(m => m.HttpContext).Returns((HttpContext?)null);

		// ACT
		string result = mockAccessor.Object.GetParameter(paramName);

		// ASSERT
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void GetParameter_WhenParameterExistsInQuery_ReturnsQueryValue()
	{
		// ARRANGE
		const string paramName = "TestParam";
		StringValues expectedValues = new(["queryValue1", "queryValue2"]);

		Mock<IQueryCollection> mockQuery = new();
		mockQuery.Setup(m => m.TryGetValue(paramName, out expectedValues)).Returns(true);

		Mock<HttpRequest> mockRequest = new();
		mockRequest.Setup(m => m.Query).Returns(mockQuery.Object);

		Mock<HttpContext> mockContext = new();
		mockContext.Setup(m => m.Request).Returns(mockRequest.Object);

		Mock<IHttpContextAccessor> mockAccessor = new();
		mockAccessor.Setup(m => m.HttpContext).Returns(mockContext.Object);

		// ACT
		string result = mockAccessor.Object.GetParameter(paramName);

		// ASSERT
		Assert.AreEqual(expectedValues.ToString(), result);
	}

	[TestMethod]
	public void GetParameter_WhenParameterExistsInHeaders_ReturnsHeaderValue()
	{
		// ARRANGE
		const string paramName = "TestParam";
		StringValues expectedValues = new(["headerValue1", "headerValue2"]);

		Mock<IQueryCollection> mockQuery = new();
		mockQuery.Setup(m => m.TryGetValue(paramName, out It.Ref<StringValues>.IsAny)).Returns(false);

		Mock<IHeaderDictionary> mockHeaders = new();
		mockHeaders.Setup(m => m.TryGetValue(paramName, out expectedValues)).Returns(true);

		Mock<HttpRequest> mockRequest = new();
		mockRequest.Setup(m => m.Query).Returns(mockQuery.Object);
		mockRequest.Setup(m => m.Headers).Returns(mockHeaders.Object);

		Mock<HttpContext> mockContext = new();
		mockContext.Setup(m => m.Request).Returns(mockRequest.Object);

		Mock<IHttpContextAccessor> mockAccessor = new();
		mockAccessor.Setup(m => m.HttpContext).Returns(mockContext.Object);

		// ACT
		string result = mockAccessor.Object.GetParameter(paramName);

		// ASSERT
		Assert.AreEqual(expectedValues.ToString(), result);
	}

	[TestMethod]
	public void GetParameter_WhenParameterDoesNotExist_ReturnsEmptyStringValues()
	{
		// ARRANGE
		const string paramName = "TestParam";
		StringValues emptyValues = StringValues.Empty;

		Mock<IQueryCollection> mockQuery = new();
		mockQuery.Setup(m => m.TryGetValue(paramName, out It.Ref<StringValues>.IsAny)).Returns(false);

		Mock<IHeaderDictionary> mockHeaders = new();
		mockHeaders.Setup(m => m.TryGetValue(paramName, out It.Ref<StringValues>.IsAny)).Returns(false);

		Mock<HttpRequest> mockRequest = new();
		mockRequest.Setup(m => m.Query).Returns(mockQuery.Object);
		mockRequest.Setup(m => m.Headers).Returns(mockHeaders.Object);

		Mock<HttpContext> mockContext = new();
		mockContext.Setup(m => m.Request).Returns(mockRequest.Object);

		Mock<IHttpContextAccessor> mockAccessor = new();
		mockAccessor.Setup(m => m.HttpContext).Returns(mockContext.Object);

		// ACT
		string result = mockAccessor.Object.GetParameter(paramName);

		// ASSERT
		Assert.AreEqual(string.Empty, result);
	}

	#endregion

	#region GetCorrelationId

	[TestMethod]
	public void GetCorrelationId_WhenValidGuidIsProvided_ReturnsCorrelationId()
	{
		// ARRANGE
		Guid expectedGuid = Guid.NewGuid();
		Mock<IHttpContextAccessor> mockAccessor = CreateHttpContextAccessorWithParameter(RequestParameters.Query.CorrelationId, expectedGuid.ToString());

		// ACT
		Guid result = mockAccessor.Object.GetCorrelationId();

		// ASSERT
		Assert.AreEqual(expectedGuid, result);
	}

	[TestMethod]
	[DataRow("invalid-guid")]
	[DataRow("")]
	public void GetCorrelationId_WhenInvalidGuidIsProvided_ReturnsEmptyGuid(string correlationId)
	{
		// ARRANGE
		Mock<IHttpContextAccessor> mockAccessor = CreateHttpContextAccessorWithParameter(RequestParameters.Query.CorrelationId, correlationId);

		// ACT
		Guid result = mockAccessor.Object.GetCorrelationId();

		// ASSERT
		Assert.AreEqual(Guid.Empty, result);
	}

	[TestMethod]
	public void GetCorrelationId_WhenParameterIsNotPresent_ReturnsEmptyGuid()
	{
		// ARRANGE
		Mock<IHttpContextAccessor> mockAccessor = CreateHttpContextAccessorWithoutParameter();

		// ACT
		Guid result = mockAccessor.Object.GetCorrelationId();

		// ASSERT
		Assert.AreEqual(Guid.Empty, result);
	}

	[TestMethod]
	public void GetCorrelationId_WhenHttpContextIsNull_ReturnsEmptyGuid()
	{
		// ARRANGE
		Mock<IHttpContextAccessor> mockAccessor = new();
		mockAccessor.Setup(m => m.HttpContext).Returns((HttpContext?)null);

		// ACT
		Guid result = mockAccessor.Object.GetCorrelationId();

		// ASSERT
		Assert.AreEqual(Guid.Empty, result);
	}

	#endregion

	#region GetLanguage

	[TestMethod]
	public void GetLanguage_WhenValidLanguageCodeIsProvided_ReturnsCultureInfo()
	{
		// ARRANGE
		const string languageCode = "en-US";
		Mock<IHttpContextAccessor> mockAccessor = CreateHttpContextAccessorWithParameter(RequestParameters.Query.Language, languageCode);

		// ACT
		CultureInfo result = mockAccessor.Object.GetLanguage();

		// ASSERT
		Assert.AreEqual(languageCode, result.Name);
	}

	[TestMethod]
	public void GetLanguage_WhenInvalidLanguageCodeIsProvided_ReturnsInvariantCulture()
	{
		// ARRANGE
		Mock<IHttpContextAccessor> mockAccessor = CreateHttpContextAccessorWithParameter(RequestParameters.Query.Language, "invalid-language");

		// ACT
		CultureInfo result = mockAccessor.Object.GetLanguage();

		// ASSERT
		Assert.AreEqual(CultureInfo.InvariantCulture, result);
	}

	[TestMethod]
	public void GetLanguage_WhenEmptyStringIsProvided_ReturnsInvariantCulture()
	{
		// ARRANGE
		Mock<IHttpContextAccessor> mockAccessor = CreateHttpContextAccessorWithParameter(RequestParameters.Query.Language, string.Empty);

		// ACT
		CultureInfo result = mockAccessor.Object.GetLanguage();

		// ASSERT
		Assert.AreEqual(CultureInfo.InvariantCulture, result);
	}

	[TestMethod]
	public void GetLanguage_WhenParameterIsNotPresent_ReturnsInvariantCulture()
	{
		// ARRANGE
		Mock<IHttpContextAccessor> mockAccessor = CreateHttpContextAccessorWithoutParameter();

		// ACT
		CultureInfo result = mockAccessor.Object.GetLanguage();

		// ASSERT
		Assert.AreEqual(CultureInfo.InvariantCulture, result);
	}

	[TestMethod]
	public void GetLanguage_WhenHttpContextIsNull_ReturnsInvariantCulture()
	{
		// ARRANGE
		Mock<IHttpContextAccessor> mockAccessor = new();
		mockAccessor.Setup(m => m.HttpContext).Returns((HttpContext?)null);

		// ACT
		CultureInfo result = mockAccessor.Object.GetLanguage();

		// ASSERT
		Assert.AreEqual(CultureInfo.InvariantCulture, result);
	}

	[TestMethod]
	[DataRow("fr-FR")]
	[DataRow("de-DE")]
	[DataRow("es-ES")]
	[DataRow("ja-JP")]
	public void GetLanguage_WhenVariousValidLanguageCodesAreProvided_ReturnsCultureInfo(string languageCode)
	{
		// ARRANGE
		Mock<IHttpContextAccessor> mockAccessor = CreateHttpContextAccessorWithParameter(RequestParameters.Query.Language, languageCode);

		// ACT
		CultureInfo result = mockAccessor.Object.GetLanguage();

		// ASSERT
		Assert.AreEqual(languageCode, result.Name);
	}

	#endregion

	#region Helper Methods

	private static Mock<IHttpContextAccessor> CreateHttpContextAccessorWithParameter(string paramName, string paramValue)
	{
		StringValues paramValues = new(paramValue);

		Mock<IQueryCollection> mockQuery = new();
		mockQuery.Setup(m => m.TryGetValue(paramName, out paramValues)).Returns(true);

		Mock<HttpRequest> mockRequest = new();
		mockRequest.Setup(m => m.Query).Returns(mockQuery.Object);

		Mock<HttpContext> mockContext = new();
		mockContext.Setup(m => m.Request).Returns(mockRequest.Object);

		Mock<IHttpContextAccessor> mockAccessor = new();
		mockAccessor.Setup(m => m.HttpContext).Returns(mockContext.Object);

		return mockAccessor;
	}

	private static Mock<IHttpContextAccessor> CreateHttpContextAccessorWithoutParameter()
	{
		Mock<IQueryCollection> mockQuery = new();
		mockQuery.Setup(m => m.TryGetValue(It.IsAny<string>(), out It.Ref<StringValues>.IsAny)).Returns(false);

		Mock<IHeaderDictionary> mockHeaders = new();
		mockHeaders.Setup(m => m.TryGetValue(It.IsAny<string>(), out It.Ref<StringValues>.IsAny)).Returns(false);

		Mock<HttpRequest> mockRequest = new();
		mockRequest.Setup(m => m.Query).Returns(mockQuery.Object);
		mockRequest.Setup(m => m.Headers).Returns(mockHeaders.Object);

		Mock<HttpContext> mockContext = new();
		mockContext.Setup(m => m.Request).Returns(mockRequest.Object);

		Mock<IHttpContextAccessor> mockAccessor = new();
		mockAccessor.Setup(m => m.HttpContext).Returns(mockContext.Object);

		return mockAccessor;
	}

	#endregion
}
