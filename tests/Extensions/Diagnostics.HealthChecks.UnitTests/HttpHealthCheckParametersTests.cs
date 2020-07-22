// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
				new HttpHealthCheckParameters(
					nameof(Constructor_SetsDefaultParameters),
					new Uri("path", UriKind.Relative),
					null,
					null,
					null,
					null,
					Array.Empty<KeyValuePair<string, object>>());

			Assert.AreEqual(HttpMethod.Get, parameters.Method, nameof(HttpHealthCheckParameters.Method));
			Assert.AreEqual(Uri.UriSchemeHttp, parameters.Scheme, nameof(HttpHealthCheckParameters.Scheme));
			Assert.AreEqual(HttpStatusCode.OK, parameters.ExpectedStatus, nameof(HttpHealthCheckParameters.ExpectedStatus));
			Assert.IsNull(parameters.AdditionalCheck, nameof(HttpHealthCheckParameters.AdditionalCheck));
			Assert.IsFalse(parameters.ReportData.Any(), nameof(HttpHealthCheckParameters.ReportData));
		}

		[TestMethod]
		public void Constructor_SetsProperties()
		{
			string endpoitName = nameof(Constructor_SetsProperties);
			Uri path = new Uri("path", UriKind.Relative);
			HttpMethod method = HttpMethod.Post;
			string scheme = Uri.UriSchemeGopher;
			HttpStatusCode status = HttpStatusCode.Ambiguous;
			Func<HttpResponseMessage, HealthCheckResult, HealthCheckResult> additionalCheck = (r, h) => HealthCheckResult.Unhealthy();
			KeyValuePair<string, object>[] reportData = new Dictionary<string, object>
			{
				{ "testKey1", new object() },
				{ "testKey2", "value" }
			}.ToArray();

			HttpHealthCheckParameters parameters =
				new HttpHealthCheckParameters(
					endpoitName,
					path,
					method,
					scheme,
					status,
					additionalCheck,
					reportData);

			Assert.AreEqual(path, parameters.RelativeUri.ToString(), nameof(HttpHealthCheckParameters.RelativeUri));
			Assert.AreEqual(method, parameters.Method, nameof(HttpHealthCheckParameters.Method));
			Assert.AreEqual(scheme, parameters.Scheme, nameof(HttpHealthCheckParameters.Scheme));
			Assert.AreEqual(status, parameters.ExpectedStatus, nameof(HttpHealthCheckParameters.ExpectedStatus));
			Assert.AreEqual(additionalCheck, parameters.AdditionalCheck, nameof(HttpHealthCheckParameters.AdditionalCheck));
			CollectionAssert.AreEquivalent(reportData, parameters.ReportData.ToArray(), nameof(HttpHealthCheckParameters.ReportData));
		}

		[TestMethod]
		public void Constructor_InvalidSheme_ThrowException()
		{
			Assert.ThrowsException<ArgumentException>(() => Create(scheme: ":)invalid"));
		}

		[TestMethod]
		public void Constructor_AbsoluteUri_ThrowException()
		{
			Assert.ThrowsException<ArgumentException>(() => Create(relatedUri: new Uri("https://localhost"), scheme: Uri.UriSchemeHttps));
		}

		[DataTestMethod]
		[DataRow("")]
		[DataRow("\t")]
		public void Constructor_InvalidSheme_ThrowException(string endpoint)
		{
			Assert.ThrowsException<ArgumentException>(() => Create(endpointName: endpoint));
		}


		internal static HttpHealthCheckParameters Create(
			string endpointName = "EndpointName",
			Uri? relatedUri = null,
			HttpMethod? method = null,
			string? scheme = null,
			HttpStatusCode expectedStatus = HttpStatusCode.OK,
			Func<HttpResponseMessage, HealthCheckResult, HealthCheckResult>? additionalCheck = null,
			KeyValuePair<string, object>[]? reportData = null) =>
				new HttpHealthCheckParameters(
					endpointName,
					relatedUri ?? new Uri("path", UriKind.Relative),
					method ?? HttpMethod.Get,
					scheme ?? Uri.UriSchemeHttp,
					expectedStatus,
					additionalCheck,
					reportData ?? Array.Empty<KeyValuePair<string, object>>());
	}
}
