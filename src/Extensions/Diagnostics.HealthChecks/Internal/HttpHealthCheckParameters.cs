// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	internal class HttpHealthCheckParameters
	{
		public string EndpointName { get; }
		public Uri RelativeUri { get; }
		public HttpMethod Method { get; }
		public string Scheme { get; }
		public Action<HttpResponseMessage, HealthCheckResult>? AdditionalCheck { get; }
		public IReadOnlyDictionary<string, object> ReportData { get; }

		public HttpHealthCheckParameters(
			string endpointName,
			Uri relatedUri,
			HttpMethod? method,
			string? scheme,
			Action<HttpResponseMessage, HealthCheckResult>? additionalCheck,
			KeyValuePair<string, object>[] reportData)
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

			AdditionalCheck = additionalCheck;

			if (reportData.Length == 0)
			{
				ReportData = s_emptyDictionary;
			}
			else
			{
				// should replaced by passing enumerable to constructor after we'll drop full framework support
				Dictionary<string, object> dictionary = new Dictionary<string, object>(reportData.Length);
				foreach (KeyValuePair<string, object> pair in reportData)
				{
					dictionary.Add(pair.Key, pair.Value);
				}

				ReportData = new ReadOnlyDictionary<string, object>(dictionary);
			}
		}

		private static readonly IReadOnlyDictionary<string, object> s_emptyDictionary =
			new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
	}
}
