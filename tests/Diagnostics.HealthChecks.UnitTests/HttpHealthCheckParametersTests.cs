// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class HttpHealthCheckParametersTests
	{
		[TestMethod]
		public void Constructor_SetsDefaultParameters()
		{
			HttpHealthCheckParameters parameters =
				new(
					new HttpRequestMessage(),
					null,
					null,
					Array.Empty<KeyValuePair<string, object>>());

			Assert.AreEqual(HttpStatusCode.OK, parameters.ExpectedStatus, nameof(HttpHealthCheckParameters.ExpectedStatus));
			Assert.IsNull(parameters.AdditionalCheck, nameof(HttpHealthCheckParameters.AdditionalCheck));
			Assert.IsFalse(parameters.ReportData.Any(), nameof(HttpHealthCheckParameters.ReportData));
		}

		[TestMethod]
		public void Constructor_SetsProperties()
		{
			HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, new Uri("path", UriKind.Relative));
			HttpStatusCode status = HttpStatusCode.Ambiguous;
			Func<HttpResponseMessage, HealthCheckResult, Task<HealthCheckResult>> additionalCheck = (r, h) =>
				Task.FromResult(HealthCheckResult.Unhealthy());
			KeyValuePair<string, object>[] reportData = new Dictionary<string, object>
			{
				{ "testKey1", new object() },
				{ "testKey2", "value" }
			}.ToArray();

			IReadOnlyDictionary<string, IEnumerable<string>> headers = new Dictionary<string, IEnumerable<string>>
			{
				{ "testHeader", new List<string> { "value" } }
			};

			HttpHealthCheckParameters parameters =
				new(
					httpRequestMessage,
					status,
					additionalCheck,
					reportData);

			Assert.AreEqual(httpRequestMessage, parameters.RequestMessage, nameof(HttpHealthCheckParameters.RequestMessage));
			Assert.AreEqual(parameters.ExpectedStatus, status, nameof(HttpHealthCheckParameters.ExpectedStatus));
			Assert.AreEqual(additionalCheck, parameters.AdditionalCheck, nameof(HttpHealthCheckParameters.AdditionalCheck));
			CollectionAssert.AreEquivalent(reportData, parameters.ReportData.ToArray(), nameof(HttpHealthCheckParameters.ReportData));
		}

		internal static HttpHealthCheckParameters Create(
			HttpRequestMessage httpRequestMessage,
			HttpStatusCode expectedStatus = HttpStatusCode.OK,
			Func<HttpResponseMessage, HealthCheckResult, Task<HealthCheckResult>>? additionalCheck = null,
			KeyValuePair<string, object>[]? reportData = null) =>
				new(
					httpRequestMessage,
					expectedStatus,
					additionalCheck,
					reportData ?? Array.Empty<KeyValuePair<string, object>>());
	}
}
