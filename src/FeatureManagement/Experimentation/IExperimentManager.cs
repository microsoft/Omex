// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Experimentation;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// An interface for managing experiments within a feature management system. This can be overridden to provide
/// custom implementations for accessing experimentation systems.
/// </summary>
public interface IExperimentManager
{
	/// <summary>
	/// Gets the status of all flights based on the provided filters.
	/// </summary>
	/// <param name="filters">The experiment filters to apply.</param>
	/// <param name="cancellationToken">The cancellation token for gracefully cancelling long-running asynchronous operations.</param>
	/// <returns>A dictionary mapping flights to the values for the specified filters.</returns>
	Task<IDictionary<string, object>> GetFlightsAsync(IDictionary<string, string> filters, CancellationToken cancellationToken);
}
