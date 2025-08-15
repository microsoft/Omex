// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A consolidator of feature gate information from multiple sources including appsettings.json and an experiment manager.
/// </summary>
public interface IFeatureGatesConsolidator
{
	/// <summary>
	/// Gets the mapping of feature gate names to their values.
	/// </summary>
	/// <param name="filters">The experiment filters to apply.</param>
	/// <param name="headerPrefix">The optional HTTP header prefix.</param>
	/// <param name="defaultPlatform">The default platform if not overridden.</param>
	/// <param name="cancellationToken">The cancellation token for gracefully cancelling long-running asynchronous operations.</param>
	/// <returns>A mapping of feature gate names to their values.</returns>
	Task<IDictionary<string, object>> GetFeatureGatesAsync(
		IDictionary<string, object> filters,
		string? headerPrefix = null,
		string? defaultPlatform = null,
		CancellationToken cancellationToken = default);
}
