// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class HttpHealthCheckParameters1 : HealthCheckParameters
	{
		public string EndpointName { get; }

		public Uri RelativeUri { get; }

		public HttpStatusCode ExpectedStatus { get; }

		public string Scheme { get; }

		public string? QueryString { get; }

		public Func<UriBuilder, HttpRequestMessage> HttpRequest { get; }

		public Func<HttpResponseMessage, HealthCheckResult, HealthCheckResult>? AdditionalCheck { get; }

		/// <summary>
		/// Creates HttpHealthCheckParameters instance
		/// </summary>
		public HttpHealthCheckParameters1(
			string endpointName,
			Uri relatedUri,
			string? scheme,
			string? queryString,
			Func<UriBuilder, HttpRequestMessage> httpRequest,
			HttpStatusCode? expectedStatus,
			Func<HttpResponseMessage, HealthCheckResult, HealthCheckResult>? additionalCheck,
			KeyValuePair<string, object>[] reportData)
				: base(reportData)
		{
			EndpointName = string.IsNullOrWhiteSpace(endpointName)
				? throw new ArgumentException("Invalid endpoint name", nameof(endpointName))
				: endpointName;

			RelativeUri = relatedUri.IsAbsoluteUri
				? throw new ArgumentException("Absolute uri not allowed", nameof(relatedUri))
				: relatedUri;

			Scheme = scheme == null
				? Uri.UriSchemeHttp
				: Uri.CheckSchemeName(scheme)
					? scheme
					: throw new ArgumentException("Invalid uri scheme", nameof(scheme));

			QueryString = queryString;

			HttpRequest = httpRequest;

			ExpectedStatus = expectedStatus ?? HttpStatusCode.OK;

			AdditionalCheck = additionalCheck;
		}
	}
}
