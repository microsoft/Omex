// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class HttpHealthCheckParameters : HealthCheckParameters
	{
		public string EndpointName { get; }

		public Uri RelativeUri { get; }

		public HttpMethod Method { get; }

		public HttpStatusCode ExpectedStatus { get; }

		public string Scheme { get; }

		public IDictionary<string, string> Headers { get; }

		public Func<HttpResponseMessage, HealthCheckResult, HealthCheckResult>? AdditionalCheck { get; }

		/// <summary>
		/// Creates HttpHealthCheckParameters instance
		/// </summary>
		/// <remarks>
		///	Parameter description provided in a public method for creation of http health check <see cref="HealthChecksBuilderExtensions.AddHttpEndpointCheck"/>
		/// </remarks>
		public HttpHealthCheckParameters(
			string endpointName,
			Uri relatedUri,
			HttpMethod? method,
			string? scheme,
			IDictionary<string, string>? headers,
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

			Method = method ?? HttpMethod.Get;

			Scheme = scheme == null
				? Uri.UriSchemeHttp
				: Uri.CheckSchemeName(scheme)
					? scheme
					: throw new ArgumentException("Invalid uri scheme", nameof(scheme));

			Headers = headers == null
				? new Dictionary<string, string>()
				: headers;

			ExpectedStatus = expectedStatus ?? HttpStatusCode.OK;

			AdditionalCheck = additionalCheck;
		}
	}
}
