﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.UnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class HttpEndpointHealthCheckTests
	{
		[TestMethod]
		public async Task CheckHealthAsync_WhenExpectedStatus_ReturnsHealthy()
		{
			string contentText = nameof(CheckHealthAsync_WhenExpectedStatus_ReturnsHealthy);
			HttpStatusCode status = HttpStatusCode.Found;
			KeyValuePair<string, object>[] reportData = new KeyValuePair<string, object>[0];
			HttpResponseMessage response = new HttpResponseMessage(status)
			{
				Content = new StringContent(contentText)
			};

			HttpHealthCheckParameters parameters = HttpHealthCheckParametersTests.Create(
				headers: new Dictionary<string, IEnumerable<string>>
				{
					{ "testHeader", new List<string> { "value" } }
				},
				expectedStatus: status,
				reportData: reportData);

			(MockClient _, HealthCheckResult result) = await RunHealthCheckAsync(parameters, response);

			Assert.AreEqual(HealthStatus.Healthy, result.Status,
				FormattableString.Invariant($"Should return {HealthStatus.Healthy} for expected status"));

			Assert.AreEqual(string.Empty, result.Description, "Content should not be in the description for healthy check");
			CollectionAssert.AreEquivalent(reportData, result.Data.ToArray(), "Result should propagate reportData");
		}

		[TestMethod]
		public async Task CheckHealthAsync_HeaderKeyIsWhiteSpace_ReturnsUnhealthy()
		{
			string contentText = nameof(CheckHealthAsync_WhenExpectedStatus_ReturnsHealthy);
			HttpStatusCode status = HttpStatusCode.Found;
			KeyValuePair<string, object>[] reportData = Array.Empty<KeyValuePair<string, object>>();
			HttpResponseMessage response = new HttpResponseMessage(status)
			{
				Content = new StringContent(contentText)
			};

			HttpHealthCheckParameters parameters = HttpHealthCheckParametersTests.Create(
				headers: new Dictionary<string, IEnumerable<string>>
				{
					{ string.Empty, new List<string> { "value" } }
				},
				expectedStatus: status,
				reportData: reportData);

			(MockClient _, HealthCheckResult result) = await RunHealthCheckAsync(parameters, response);

			Assert.AreEqual(HealthStatus.Unhealthy, result.Status,
				FormattableString.Invariant($"Should return {HealthStatus.Unhealthy} for expected status"));
		}

		[TestMethod]
		public async Task CheckHealthAsync_HeaderValueIsEmpty_ReturnsHealthy()
		{
			string contentText = nameof(CheckHealthAsync_WhenExpectedStatus_ReturnsHealthy);
			HttpStatusCode status = HttpStatusCode.Found;
			KeyValuePair<string, object>[] reportData = Array.Empty<KeyValuePair<string, object>>();
			HttpResponseMessage response = new HttpResponseMessage(status)
			{
				Content = new StringContent(contentText)
			};

			HttpHealthCheckParameters parameters = HttpHealthCheckParametersTests.Create(
				headers: new Dictionary<string, IEnumerable<string>>
				{
					{ "testheader", new List<string>() }
				},
				expectedStatus: status,
				reportData: reportData);

			(MockClient _, HealthCheckResult result) = await RunHealthCheckAsync(parameters, response);

			Assert.AreEqual(HealthStatus.Healthy, result.Status,
				FormattableString.Invariant($"Should return {HealthStatus.Healthy} for expected status"));

			Assert.AreEqual(string.Empty, result.Description, "Content should not be in the description for healthy check");
			CollectionAssert.AreEquivalent(reportData, result.Data.ToArray(), "Result should propagate reportData");
		}

		[TestMethod]
		public async Task CheckHealthAsync_MultipleHeaderValues_ReturnsHealthy()
		{
			string contentText = nameof(CheckHealthAsync_WhenExpectedStatus_ReturnsHealthy);
			HttpStatusCode status = HttpStatusCode.Found;
			KeyValuePair<string, object>[] reportData = Array.Empty<KeyValuePair<string, object>>();
			HttpResponseMessage response = new HttpResponseMessage(status)
			{
				Content = new StringContent(contentText)
			};

			HttpHealthCheckParameters parameters = HttpHealthCheckParametersTests.Create(
				headers: new Dictionary<string, IEnumerable<string>>
				{
					{ "testheader", new List<string> { "value1", "value2" } },
					{ "testheader2", new List<string> { "value1", "value2" } }
				},
				expectedStatus: status,
				reportData: reportData);

			(MockClient _, HealthCheckResult result) = await RunHealthCheckAsync(parameters, response);

			Assert.AreEqual(HealthStatus.Healthy, result.Status,
				FormattableString.Invariant($"Should return {HealthStatus.Healthy} for expected status"));

			Assert.AreEqual(string.Empty, result.Description, "Content should not be in the description for healthy check");
			CollectionAssert.AreEquivalent(reportData, result.Data.ToArray(), "Result should propagate reportData");
		}

		[TestMethod]
		public async Task CheckHealthAsync_WhenWrongStatusAndDefaultRegistrationFailureStatus_ReturnsUnhealthy()
		{
			string contentText = nameof(CheckHealthAsync_WhenWrongStatusAndDefaultRegistrationFailureStatus_ReturnsUnhealthy);
			KeyValuePair<string, object>[] reportData = new KeyValuePair<string, object>[0];
			HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
			{
				Content = new StringContent(contentText)
			};

			HttpHealthCheckParameters parameters = HttpHealthCheckParametersTests.Create(
				expectedStatus: HttpStatusCode.NotFound,
				reportData: reportData);

			(MockClient _, HealthCheckResult result) = await RunHealthCheckAsync(parameters, response);

			Assert.AreEqual(HealthStatus.Unhealthy, result.Status,
				FormattableString.Invariant($"Should return {HealthStatus.Unhealthy} for wrong status"));

			Assert.AreEqual(contentText, result.Description, "Content should be in the description for unhealthy check");
			CollectionAssert.AreEquivalent(reportData, result.Data.ToArray(), "Result should propagate reportData");
		}

		[TestMethod]
		public async Task CheckHealthAsync_WhenWrongStatusAndExplicitRegistrationFailureStatus_ReturnsRegistrationFailureStatus()
		{
			string contentText = nameof(CheckHealthAsync_WhenWrongStatusAndExplicitRegistrationFailureStatus_ReturnsRegistrationFailureStatus);
			KeyValuePair<string, object>[] reportData = new KeyValuePair<string, object>[0];
			HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
			{
				Content = new StringContent(contentText)
			};

			HttpHealthCheckParameters parameters = HttpHealthCheckParametersTests.Create(
				expectedStatus: HttpStatusCode.NotFound,
				reportData: reportData);

			(MockClient _, HealthCheckResult result) = await RunHealthCheckAsync(parameters, response, failureStatus: HealthStatus.Degraded);

			Assert.AreEqual(HealthStatus.Degraded, result.Status,
				FormattableString.Invariant($"Should return {HealthStatus.Unhealthy} for wrong status"));

			Assert.AreEqual(contentText, result.Description, "Content should be in the description for unhealthy check");
			CollectionAssert.AreEquivalent(reportData, result.Data.ToArray(), "Result should propagate reportData");
		}

		[TestMethod]
		public async Task CheckHealthAsync_WithAdditionalCheck_ReturnsOverridenResult()
		{
			string contentText = nameof(CheckHealthAsync_WithAdditionalCheck_ReturnsOverridenResult);
			KeyValuePair<string, object>[] reportData = new KeyValuePair<string, object>[0];
			HttpResponseMessage expectedResponse = new HttpResponseMessage(HttpStatusCode.Ambiguous)
			{
				Content = new StringContent(contentText)
			};

			HttpResponseMessage? actualResponce = null;
			HealthCheckResult initialResult = default;
			HealthCheckResult expectedResult = HealthCheckResult.Healthy("Some description", new Dictionary<string, object>());

			HttpHealthCheckParameters parameters = HttpHealthCheckParametersTests.Create(
				expectedStatus: HttpStatusCode.BadRequest,
				additionalCheck: (response, result) =>
				{
					actualResponce = response;
					initialResult = result;
					return expectedResult;
				},
				reportData: reportData);

			(MockClient _, HealthCheckResult result) = await RunHealthCheckAsync(parameters, expectedResponse);

			Assert.AreEqual(expectedResult, result, "Should return overridden result");
			Assert.AreEqual(expectedResponse, actualResponce, "Should provide proper HttpResponse to function");
			Assert.AreEqual(HealthStatus.Unhealthy, initialResult.Status, "Provided result should have proper status");
			Assert.AreEqual(contentText, initialResult.Description, "Provided result should have proper description");
			CollectionAssert.AreEquivalent(reportData, result.Data.ToArray(), "Provided result should have proper data");
		}

		private async Task<(MockClient, HealthCheckResult)> RunHealthCheckAsync(
			HttpHealthCheckParameters parameters, HttpResponseMessage response, HealthStatus failureStatus = HealthStatus.Unhealthy)
		{
			Mock<IHttpClientFactory> factoryMock = new();
			MockClient clientMock = new(response);
			const int samplePort = 6789;
			SfConfigurationProviderHelper.SetPortVariable(parameters.EndpointName, samplePort);

			factoryMock.Setup(f => f.CreateClient(HttpEndpointHealthCheck.HttpClientLogicalName))
				.Returns(clientMock);

			HttpEndpointHealthCheck healthCheck = new HttpEndpointHealthCheck(
				parameters,
				factoryMock.Object,
				new NullLogger<HttpEndpointHealthCheck>(),
				new ActivitySource("Test"));

			HealthCheckContext checkContext = HealthCheckContextHelper.CreateCheckContext();
			checkContext.Registration.FailureStatus = failureStatus;

			HealthCheckResult result = await healthCheck.CheckHealthAsync(checkContext);

			return (clientMock, result);
		}

		public class MockClient : HttpClient
		{
			private readonly HttpResponseMessage m_responseMessage;

			public MockClient(HttpResponseMessage responseMessage) => m_responseMessage = responseMessage;

			public HttpRequestMessage? Request { get; private set; }

			public CancellationToken CancellationToken { get; private set; }

			public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				Request = request;
				CancellationToken = cancellationToken;
				return Task.FromResult(m_responseMessage);
			}
		}
	}
}
