// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// A series of constants used by health checks.
/// </summary>
internal static class HealthCheckConstants
{
	/// <summary>
	/// The local service default host.
	/// </summary>
	public const string LocalServiceDefaultHost = "localhost";

	/// <summary>
	/// The HTTP client logical name.
	/// </summary>
	public const string HttpClientLogicalName = "HttpEndpointHealthCheckHttpClient";
}
