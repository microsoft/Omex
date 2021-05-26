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
		public HttpStatusCode ExpectedStatus { get; }

		public HttpRequestMessage HttpRequest { get; }

		public Func<HttpResponseMessage, HealthCheckResult, HealthCheckResult>? AdditionalCheck { get; }

		/// <summary>
		/// Creates HttpHealthCheckParameters instance
		/// </summary>
		public HttpHealthCheckParameters(
			HttpRequestMessage httpRequest,
			HttpStatusCode? expectedStatus,
			Func<HttpResponseMessage, HealthCheckResult, HealthCheckResult>? additionalCheck,
			KeyValuePair<string, object>[] reportData)
				: base(reportData)
		{
			HttpRequest = httpRequest;

			ExpectedStatus = expectedStatus ?? HttpStatusCode.OK;

			AdditionalCheck = additionalCheck;
		}
	}
}
