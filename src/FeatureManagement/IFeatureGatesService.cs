// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Microsoft.Omex.Extensions.FeatureManagement
{
	/// <summary>
	/// The service managing the feature gates.
	/// </summary>
	/// <remarks>This service is responsible for managing feature gates (feature flags) and experimentation
	/// within the app. It provides APIs to query which features are enabled, which are blocked, and which experimental
	/// treatments apply to a customer, supporting both static feature flags and dynamic experimentation
	/// scenarios.</remarks>
	public interface IFeatureGatesService
	{
		/// <summary>
		/// Gets the list of features which are explicitly allowed for this request as a <see cref="string"/>.
		/// </summary>
		string RequestedFeatures { get; }

		/// <summary>
		/// Gets the list of features which are explicitly blocked for this request as a <see cref="string"/>.
		/// </summary>
		string BlockedFeatures { get; }

		/// <summary>
		/// Gets all the feature gates and their values.
		/// </summary>
		/// <returns>A dictionary mapping the feature gate names to their values.</returns>
		Task<IDictionary<string, object>> GetFeatureGatesAsync();

		/// <summary>
		/// Gets the experiments that apply to the the customer.
		/// </summary>
		/// <param name="filters">The experiment filters to apply.</param>
		/// <param name="cancellationToken">The cancellation token for gracefully cancelling long-running asynchronous operations.</param>
		/// <returns>The values of the experiments that apply to the customer.</returns>
		Task<IDictionary<string, object>> GetExperimentalFeaturesAsync(IDictionary<string, object> filters, CancellationToken cancellationToken);

		/// <summary>
		/// Gets the feature gate value associated with the assigned experiment flight.
		/// </summary>
		/// <remarks>In most cases, the experiment flights will be configured with a Boolean value. String values can
		/// also be used as a way of sending configuration to the service code for the flight.</remarks>
		/// <param name="featureGate">The feature gate.</param>
		/// <param name="filters">The experiment filters to apply.</param>
		/// <param name="cancellationToken">The cancellation token for gracefully cancelling long-running asynchronous operations.</param>
		/// <returns>The status of the feature gate.</returns>
		Task<FeatureGateResult> GetExperimentFeatureValueAsync(string featureGate, IDictionary<string, object> filters, CancellationToken cancellationToken);

		/// <summary>
		/// Checks if the feature gate is active.
		/// </summary>
		/// <param name="featureGate">The feature gate.</param>
		/// <returns><c>true</c> if the feature gate is active.</returns>
		Task<bool> IsFeatureGateApplicableAsync(string featureGate);

		/// <summary>
		/// Checks if the customers has been assigned to the specified experiment.
		/// </summary>
		/// <param name="featureGate">The feature gate.</param>
		/// <param name="filters">The experiment filters to apply.</param>
		/// <param name="cancellationToken">The cancellation token for gracefully cancelling long-running asynchronous operations.</param>
		/// <returns><c>true</c> if the customer has been allocated to the experiment treatment.</returns>
		Task<bool> IsExperimentApplicableAsync(string featureGate, IDictionary<string, object> filters, CancellationToken cancellationToken);
	}

}
