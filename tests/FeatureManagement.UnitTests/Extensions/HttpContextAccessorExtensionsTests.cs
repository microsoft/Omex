// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests.Extensions;

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;
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
		string action() => mockAccessor.Object.GetParameter(paramName);

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
}
