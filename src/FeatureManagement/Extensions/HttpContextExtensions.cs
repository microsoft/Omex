// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Net;
using DeviceDetectorNET;
using DeviceDetectorNET.Results.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.Omex.Extensions.FeatureManagement.Constants;
using Microsoft.Omex.Extensions.FeatureManagement.Types;

namespace Microsoft.Omex.Extensions.FeatureManagement.Extensions
{
	/// <summary>
	/// Extension methods for <see cref="HttpContext"/>.
	/// </summary>
	internal static class HttpContextExtensions
	{
		/// <summary>
		/// Gets the browser information from the HTTP context.
		/// </summary>
		/// <param name="httpContext">The HTTP context.</param>
		/// <returns>The browser information.</returns>
		public static string GetBrowser(this HttpContext httpContext)
		{
			if (!httpContext.Request.Headers.TryGetValue(HeaderNames.UserAgent, out StringValues userAgent) ||
				string.IsNullOrWhiteSpace(userAgent.ToString()))
			{
				return string.Empty;
			}

			DeviceDetector deviceDetector = new(userAgent);
			deviceDetector.Parse();
			BrowserMatchResult match = deviceDetector.GetBrowserClient().Match;
			return match == null
				? string.Empty
				: match.Name;
		}

		/// <summary>
		/// Gets the device type from the HTTP context.
		/// </summary>
		/// <param name="httpContext">The HTTP context.</param>
		/// <returns>The device type.</returns>
		public static DeviceType GetDeviceType(this HttpContext httpContext)
		{
			if (!httpContext.Request.Headers.TryGetValue(HeaderNames.UserAgent, out StringValues userAgent) ||
				string.IsNullOrWhiteSpace(userAgent.ToString()))
			{
				return DeviceType.Unknown;
			}

			return httpContext switch
			{
				_ when httpContext.IsDesktopClient(userAgent) =>
					DeviceType.Desktop,
				_ when httpContext.IsMobileClient(userAgent) =>
					DeviceType.Mobile,
				_ =>
					DeviceType.Unknown
			};
		}

		/// <summary>
		/// Retrieves the partner from the HTTP context headers.
		/// </summary>
		/// <param name="httpContext">The HTTP context.</param>
		/// <param name="headerPrefix">The optional HTTP header prefix.</param>
		/// <returns>The partner.</returns>
		public static string GetPartner(this HttpContext httpContext, string? headerPrefix = null)
		{
			string headerName = GetPartnerHeader(headerPrefix);
			if (!httpContext.Request.Headers.TryGetValue(headerName, out StringValues partner) ||
				string.IsNullOrWhiteSpace(partner.ToString()))
			{
				return string.Empty;
			}

			return partner.ToString().Trim();
		}

		/// <summary>
		/// Retrieves the platform from the HTTP context headers.
		/// </summary>
		/// <param name="httpContext">The HTTP context.</param>
		/// <param name="headerPrefix">The optional HTTP header prefix.</param>
		/// <param name="defaultPlatform">The default platform if not overridden.</param>
		/// <returns>The platform.</returns>
		public static string GetPlatform(this HttpContext httpContext, string? headerPrefix = null, string? defaultPlatform = null)
		{
			string headerName = GetPlatformHeader(headerPrefix);
			if (!httpContext.Request.Headers.TryGetValue(headerName, out StringValues platform) ||
				string.IsNullOrWhiteSpace(platform.ToString()))
			{
				return string.IsNullOrWhiteSpace(defaultPlatform)
					? string.Empty
					: defaultPlatform;
			}

			return platform.ToString().Trim();
		}

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
			if (
				!httpContext.Request.Headers.TryGetValue(
					RequestParameters.Header.ForwardedFor,
					out StringValues headerValues
				)
			)
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

		private static bool IsDesktopClient(this HttpContext httpContext, StringValues userAgent) =>
			IsClientType(httpContext, userAgent, detector => detector.IsDesktop());

		private static bool IsMobileClient(this HttpContext httpContext, StringValues userAgent) =>
			IsClientType(httpContext, userAgent, detector => detector.IsMobile());

		private static bool IsClientType(HttpContext httpContext, StringValues userAgent, Func<DeviceDetector, bool> deviceCheck)
		{
			DeviceDetector deviceDetector = new(userAgent);
			deviceDetector.Parse();
			return deviceCheck(deviceDetector);
		}

		private static string GetPartnerHeader(string? headerPrefix) =>
			GetHeader(RequestParameters.Header.Partner, headerPrefix);

		private static string GetPlatformHeader(string? headerPrefix) =>
			GetHeader(RequestParameters.Header.Platform, headerPrefix);

		private static string GetHeader(string header, string? headerPrefix) =>
			string.IsNullOrWhiteSpace(headerPrefix)
				? header
				: $"{headerPrefix}-{header}";
	}
}
