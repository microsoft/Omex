// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Extensions;

using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Omex.Extensions.FeatureManagement.Constants;

/// <summary>
/// Extension methods for <see cref="HttpContext"/>.
/// </summary>
internal static class HttpContextExtensions
{
	/// <summary>
	/// Retrieves the partner from the HTTP context headers.
	/// </summary>
	/// <param name="httpContext">The HTTP context.</param>
	/// <param name="headerPrefix">The optional HTTP header prefix.</param>
	/// <returns>The partner.</returns>
	public static string GetPartner(this HttpContext httpContext, string? headerPrefix = null) =>
		GetHeaderValue(httpContext, RequestParameters.Header.Partner, headerPrefix);

	/// <summary>
	/// Retrieves the platform from the HTTP context headers.
	/// </summary>
	/// <param name="httpContext">The HTTP context.</param>
	/// <param name="headerPrefix">The optional HTTP header prefix.</param>
	/// <param name="defaultPlatform">The default platform if not overridden.</param>
	/// <returns>The platform.</returns>
	public static string GetPlatform(this HttpContext httpContext, string? headerPrefix = null, string? defaultPlatform = null) =>
		GetHeaderValue(httpContext, RequestParameters.Header.Platform, headerPrefix, defaultPlatform);

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

	/// <summary>
	/// Checks if the HTTP request is local.
	/// </summary>
	/// <param name="httpContext">The HTTP context.</param>
	/// <returns>A value indicating whether the HTTP request is local.</returns>
	public static bool IsLocal(this HttpContext httpContext)
	{
		IPAddress forwardedAddress = httpContext.GetForwardedAddress();
		if (forwardedAddress != IPAddress.None && !IPAddress.IsLoopback(forwardedAddress))
		{
			return false;
		}

		ConnectionInfo connection = httpContext.Connection;
		if (connection.RemoteIpAddress is not null)
		{
			return connection.LocalIpAddress is not null
				? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
				: IPAddress.IsLoopback(connection.RemoteIpAddress);
		}

		// Handle the case for the in-memory test server or when dealing with default connection info.
		// At this point, connection.RemoteIpAddress will always be null.
		return connection.LocalIpAddress is null;
	}

	/// <summary>
	/// Gets the forwarded for IP address if the request has been proxied.
	/// </summary>
	/// <param name="httpContext">The HTTP context for the request.</param>
	/// <returns>The forwarded addresses or <see cref="IPAddress.None"/> if none was present.</returns>
	public static IPAddress GetForwardedAddress(this HttpContext httpContext)
	{
		if (!httpContext.Request.Headers.TryGetValue(RequestParameters.Header.ForwardedFor, out StringValues headerValues))
		{
			return IPAddress.None;
		}

		foreach (string? headerValue in headerValues)
		{
			if (!string.IsNullOrWhiteSpace(headerValue))
			{
				string[] addresses = headerValue.Split(',');

				// Where multiple proxies are involved, the last IP address in the list is typically the client's
				// IP address. Iterate through the list in reverse order to prioritize the most relevant IP address.
				for (int i = addresses.Length - 1; i >= 0; i--)
				{
					// The application gateway appends the port on which the request was received to the client IP.
					string[] headerParts = addresses[i].Split(':');
					if (IPAddress.TryParse(headerParts[0], out IPAddress? result))
					{
						return result;
					}
				}
			}
		}

		return IPAddress.None;
	}

	private static string GetHeaderValue(HttpContext httpContext, string header, string? headerPrefix, string? defaultValue = null)
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
