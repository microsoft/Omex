// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.FeatureManagement.Experimentation
{
	/// <summary>
	/// An interface for managing experiments within a feature management system. This can be overridden to provide
	/// custom implementations for accessing experimentation systems.
	/// </summary>
	public interface IExperimentManager
	{
		/// <summary>
		/// Gets the status of all experiments based on the provided filters.
		/// </summary>
		/// <param name="filters">The experiment filters to apply.</param>
		/// <param name="cancellationToken">The cancellation token for gracefully cancelling long-running asynchronous operations.</param>
		/// <returns>A dictionary mapping feature gates to the values for the specified filters.</returns>
		Task<IDictionary<string, bool>> GetExperimentStatusesAsync(ExperimentFilters filters, CancellationToken cancellationToken);
	}
}
