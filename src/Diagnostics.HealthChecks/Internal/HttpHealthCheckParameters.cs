﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class HttpHealthCheckParameters : HealthCheckParameters
	{
		public HttpStatusCode ExpectedStatus { get; }

		public HttpRequestMessage RequestMessage { get; }

		public Func<HttpResponseMessage, HealthCheckResult, Task<HealthCheckResult>>? AdditionalCheck { get; }

		/// <summary>
		/// Creates HttpHealthCheckParameters instance
		/// </summary>
		/// <remarks>
		/// Parameter description provided in a public method for creation of http health check <see cref="HealthChecksBuilderExtensions"/>
		/// </remarks>
		public HttpHealthCheckParameters(
			HttpRequestMessage httpRequestMessage,
			HttpStatusCode? expectedStatus,
			Func<HttpResponseMessage, HealthCheckResult, Task<HealthCheckResult>>? additionalCheck,
			KeyValuePair<string, object>[] reportData)
				: base(reportData)
		{
			RequestMessage = httpRequestMessage;

			ExpectedStatus = expectedStatus ?? HttpStatusCode.OK;

			AdditionalCheck = additionalCheck;
		}
	}
}
