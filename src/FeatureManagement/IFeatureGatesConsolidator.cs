// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.FeatureManagement.Types;

namespace Microsoft.Omex.Extensions.FeatureManagement
{
	/// <summary>
	/// A consolidator of feature gate information from multiple sources including appsettings.json and an experimentation manager.
	/// </summary>
	public interface IFeatureGatesConsolidator
	{
		/// <summary>
		/// Gets the mapping of feature gate names to their values, retrieving the customer ID from the currently signed-in Entra ID.
		/// </summary>
		/// <param name="headerPrefix">The optional HTTP header prefix.</param>
		/// <param name="defaultPlatform">The default platform if not overridden.</param>
		/// <param name="cancellationToken">The cancellation token for gracefully cancelling long-running asynchronous operations.</param>
		/// <returns>A mapping of feature gate names to their values.</returns>
		Task<IDictionary<string, bool>> GetFeatureGatesForEntraIdAsync(
			string? headerPrefix = null,
			string? defaultPlatform = null,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Gets the mapping of feature gate names to their values.
		/// </summary>
		/// <param name="customerId">The optional customer ID.</param>
		/// <param name="headerPrefix">The optional HTTP header prefix.</param>
		/// <param name="defaultPlatform">The default platform if not overridden.</param>
		/// <param name="cancellationToken">The cancellation token for gracefully cancelling long-running asynchronous operations.</param>
		/// <returns>A mapping of feature gate names to their values.</returns>
		Task<IDictionary<string, bool>> GetFeatureGatesAsync(
			CustomerId? customerId = null,
			string? headerPrefix = null,
			string? defaultPlatform = null,
			CancellationToken cancellationToken = default);
	}
}
