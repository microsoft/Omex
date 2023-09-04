// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.AspNetCore;

/// <summary>
/// Expands the <see cref="HealthCheckParameters"/> class wih additional parameters required by the
/// <see cref="EndpointLivenessHealthCheck"/> class.
/// </summary>
public class EndpointLivenessHealthCheckParameters : HealthCheckParameters
{
	/// <summary>
	/// The Service Fabric endpoint name, it will be used to fetch the service port.
	/// </summary>
	public string EndpointName { get; }

	/// <summary>
	/// The endpoint relative url at which the Service Fabric health check will be reachable.
	/// </summary>
	public string EndpointRelativeUri { get; }

	/// <summary>
	/// The name of the <seealso cref="HttpClient"/> that will be used to create the instance used
	/// by the health check from <seealso cref="IHttpClientFactory"/>.
	/// </summary>
	public string HttpClientLogicalName { get; }

	/// <summary>
	/// The name of the service host that will be used to perform the health check HTTP call,
	/// usually equal to `localhost`.
	/// </summary>
	public string Host { get; }

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="endpointName">The Service Fabric endpoint name.</param>
	/// <param name="httpClientLogicalName">The name of the <seealso cref="HttpClient"/> defined in the consumer service DI.</param>
	/// <param name="endpointRelativeUrl">
	/// The endpoint relative url at which the Service Fabric health check will be reachable.
	/// </param>
	/// <param name="host">The host used to perform the health check HTTP call to the service.</param>
	/// <param name="reportData">The report data.</param>
	public EndpointLivenessHealthCheckParameters(
		string endpointName,
		string httpClientLogicalName,
		string endpointRelativeUrl,
		string host = "localhost",
		params KeyValuePair<string, object>[] reportData)
		: base(reportData)
	{
		EndpointName = endpointName;
		HttpClientLogicalName = httpClientLogicalName;
		EndpointRelativeUri = endpointRelativeUrl;
		Host = host;
	}
}
