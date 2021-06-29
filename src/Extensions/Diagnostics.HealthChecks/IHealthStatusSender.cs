// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	/// <summary>
	/// Interface for sending health status from HealthPublisher to extenal consumer (ex. SF Cluster).
	/// </summary>
	public interface IHealthStatusSender
	{
		/// <summary>
		/// Initialization method that's called before each health status publishing session.
		/// </summary>
		/// <returns>
		/// Returns true if initialization was succesful and false if it wan't and user should retry later.
		/// </returns>
		Task<bool> IntializeAsync(CancellationToken token);

		/// <summary>
		/// Sends health status of single health check.
		/// </summary>
		Task SendStatusAsync(string checkName, HealthStatus status, string description, CancellationToken token);
	}
}
