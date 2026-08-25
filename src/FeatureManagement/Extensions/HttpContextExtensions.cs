// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Extensions;

using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Omex.Extensions.FeatureManagement.Constants;

/// <summary>
/// Extension methods for <see cref="HttpContext"/>.
/// </summary>
internal static class HttpContextExtensions
{
	/// <summary>
	/// Retrieves the partner and platform information from the HTTP context headers.
	/// </summary>
	/// <param name="httpContext">The HTTP context.</param>
	/// <param name="headerPrefix">The optional HTTP header prefix.</param>
	/// <param name="defaultPlatform">The default platform if not overridden.</param>
	/// <returns>A string containing the partner and platform information in the format of "partner/platform"</returns>
	public static string GetPartnerInfo(this HttpContext httpContext, string? headerPrefix = null, string? defaultPlatform = null)
	{
		string partner = httpContext.GetPartner(headerPrefix);
		string platform = httpContext.GetPlatform(headerPrefix, defaultPlatform);

		return string.Join("/", new[] { partner, platform }.Where(s => !string.IsNullOrWhiteSpace(s)));
	}

	private static string GetPartner(this HttpContext httpContext, string? headerPrefix) =>
		GetHeaderValue(httpContext, RequestParameters.Header.Partner, headerPrefix, null);

	private static string GetPlatform(this HttpContext httpContext, string? headerPrefix, string? defaultPlatform) =>
		GetHeaderValue(httpContext, RequestParameters.Header.Platform, headerPrefix, defaultPlatform);

	private static string GetHeaderValue(HttpContext httpContext, string header, string? headerPrefix, string? defaultValue)
	{
		string headerName = string.IsNullOrWhiteSpace(headerPrefix)
			? header
			: $"{headerPrefix}-{header}";

		if (!httpContext.Request.Headers.TryGetValue(headerName, out StringValues value) ||
			string.IsNullOrWhiteSpace(value.ToString()))
		{
			return string.IsNullOrWhiteSpace(defaultValue)
				? string.Empty
				: defaultValue;
		}

		return value.ToString().Trim();
	}
}
