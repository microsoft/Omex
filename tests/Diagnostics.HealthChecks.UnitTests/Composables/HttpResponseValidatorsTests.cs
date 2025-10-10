// Copyright (C) Microsoft Corporation. All rights reserved.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Composables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests.Composables
{
	[TestClass]
	public class HttpResponseValidatorsTests
	{
		[TestMethod]
		public async Task CheckResponseStatusCodeAsync_ReturnsHealthy_WhenResponseIsHealthy()
		{
			Mock<IHealthCheck> mockedHealthCheck = new();
			HealthCheckContext context = HealthCheckTestHelpers.GetHealthCheckContext(mockedHealthCheck.Object);
			HttpResponseMessage response = new(HttpStatusCode.OK);

			HealthCheckResult result = await HealthCheckComposablesExtensions.CheckResponseStatusCodeAsync(context, response);

			Assert.AreEqual(HealthStatus.Healthy, result.Status);
		}

		[TestMethod]
		[DataRow(HealthStatus.Unhealthy)]
		[DataRow(HealthStatus.Degraded)]
		public async Task CheckResponseStatusCodeAsync_ReturnsHealthy_WhenStatusCodeIsAmongSpecifiedOnes(HealthStatus faultStatus)
		{
			Mock<IHealthCheck> mockedHealthCheck = new();
			HttpStatusCode[] allowedStatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest };
			HealthCheckContext context = HealthCheckTestHelpers.GetHealthCheckContext(mockedHealthCheck.Object, faultStatus);
			HttpResponseMessage response1 = new(HttpStatusCode.OK);
			HttpResponseMessage response2 = new(HttpStatusCode.BadRequest);
			HttpResponseMessage response3 = new(HttpStatusCode.InternalServerError);

			HealthCheckResult result1 = await HealthCheckComposablesExtensions.CheckResponseStatusCodeAsync(context, response1, allowedStatusCodes);
			HealthCheckResult result2 = await HealthCheckComposablesExtensions.CheckResponseStatusCodeAsync(context, response2, allowedStatusCodes);
			HealthCheckResult result3 = await HealthCheckComposablesExtensions.CheckResponseStatusCodeAsync(context, response3, allowedStatusCodes);

			Assert.AreEqual(HealthStatus.Healthy, result1.Status);
			Assert.AreEqual(HealthStatus.Healthy, result2.Status);
			Assert.AreEqual(faultStatus, result3.Status);
		}

		[TestMethod]
		[DataRow(HttpStatusCode.BadRequest, HealthStatus.Unhealthy)]
		[DataRow(HttpStatusCode.Forbidden, HealthStatus.Unhealthy)]
		[DataRow(HttpStatusCode.InternalServerError, HealthStatus.Unhealthy)]
		[DataRow(HttpStatusCode.Unauthorized, HealthStatus.Unhealthy)]
		[DataRow(HttpStatusCode.BadRequest, HealthStatus.Degraded)]
		[DataRow(HttpStatusCode.Forbidden, HealthStatus.Degraded)]
		[DataRow(HttpStatusCode.InternalServerError, HealthStatus.Degraded)]
		[DataRow(HttpStatusCode.Unauthorized, HealthStatus.Degraded)]
		public async Task OkHttpResponseValidator_ReturnsHealthy_WhenResponseIsHealthy(
			HttpStatusCode responseStatusCode,
			HealthStatus registeredHealthStatus)
		{
			Mock<IHealthCheck> mockedHealthCheck = new();
			HealthCheckContext context = HealthCheckTestHelpers.GetHealthCheckContext(mockedHealthCheck.Object, registeredHealthStatus);
			HttpResponseMessage response = new(responseStatusCode);

			HealthCheckResult result = await HealthCheckComposablesExtensions.CheckResponseStatusCodeAsync(context, response);

			Assert.AreEqual(registeredHealthStatus, result.Status);
			Assert.IsTrue(result.Description?.Contains(responseStatusCode.ToString()));
		}
	}
}
