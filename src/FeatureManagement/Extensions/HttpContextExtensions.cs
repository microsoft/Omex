// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Extensions;

using System;
using System.Collections.Generic;
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
	private static readonly Dictionary<IPAddressType, Func<string, IPAddress?>> s_ipParseStrategies = new()
	{
		[IPAddressType.IPv4] = TryParseIPAddress,
		[IPAddressType.IPv4WithPort] = ParseIPv4WithPort,
		[IPAddressType.IPv6] = TryParseIPAddress,
		[IPAddressType.IPv6WithPort] = ParseIPv6WithPort,
	};

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
			if (string.IsNullOrWhiteSpace(headerValue))
			{
				continue;
			}

			IPAddress? result = ParseForwardedHeaderValue(headerValue);
			if (result != null)
			{
				return result;
			}
		}

		return IPAddress.None;
	}

	private static IPAddress? ParseForwardedHeaderValue(string headerValue)
	{
		string[] addresses = headerValue.Split(',');

		// Where multiple proxies are involved, the last IP address in the list is typically the client's
		// IP address. Iterate through the list in reverse order to prioritize the most relevant IP address.
		for (int i = addresses.Length - 1; i >= 0; i--)
		{
			string address = addresses[i].Trim();

			// The application gateway appends the port on which the request was received to the client IP.
			IPAddressType addressType = DetermineIPAddressType(address);
			IPAddress? result = s_ipParseStrategies[addressType](address);
			if (result != null)
			{
				return result;
			}
		}

		return null;
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

	private static IPAddressType DetermineIPAddressType(string address)
	{
		// Handle IPv6 with port, e.g., [::1]:8080.
		if (address.StartsWith('[') && address.Contains(']'))
		{
			return IPAddressType.IPv6WithPort;
		}

		int colonCount = address.Count(c => c == ':');
		return colonCount switch
		{
			0 => IPAddressType.IPv4,
			1 => IPAddressType.IPv4WithPort,
			_ => IPAddressType.IPv6,
		};
	}

	private static IPAddress? TryParseIPAddress(string address) =>
		IPAddress.TryParse(address, out IPAddress? result)
			? result
			: null;

	private static IPAddress? ParseIPv4WithPort(string address)
	{
		string[] headerParts = address.Split(':');
		return TryParseIPAddress(headerParts[0]);
	}

	private static IPAddress? ParseIPv6WithPort(string address)
	{
		int bracketEnd = address.IndexOf(']');
		string ipv6Address = address[1..bracketEnd];
		return TryParseIPAddress(ipv6Address);
	}

	private enum IPAddressType
	{
		IPv4,
		IPv4WithPort,
		IPv6,
		IPv6WithPort,
	}
}
