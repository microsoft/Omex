// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The service managing the feature gates.
/// </summary>
/// <remarks>
/// This service is responsible for managing feature gates (feature flags) and experimentation within the application.
/// It provides APIs to query which features are enabled, which are blocked, and which experimental treatments apply to
/// a customer, supporting both static feature flags and dynamic experimentation scenarios.
///
/// The service integrates with both a feature manager (for static configuration-based features) and an experiment
/// manager (for dynamic, customer-targeted features). Features can be overridden via query-string parameters.
///
/// Features prefixed with "FE_" are treated as frontend features, and this prefix is automatically stripped when
/// returning feature-gate values to clients.
/// </remarks>
public interface IFeatureGatesService
{
	/// <summary>
	/// Gets the list of features which are explicitly allowed for this request as a semicolon-delimited <see cref="string"/>.
	/// </summary>
	/// <remarks>
	/// This property returns features that have been explicitly enabled via query-string overrides or configuration
	/// settings. The format is a semicolon-delimited string (e.g., "Feature1;Feature2;FE_Feature3"). Frontend features
	/// may include the "FE_" prefix in this list.
	/// </remarks>
	/// <example>
	/// Example value: "FE_NewUI;FE_BetaFeature;BackendOptimization"
	/// </example>
	string RequestedFeatures { get; }

	/// <summary>
	/// Gets the list of features which are explicitly blocked for this request as a semicolon-delimited <see cref="string"/>.
	/// </summary>
	/// <remarks>
	/// This property returns features that have been explicitly disabled via query-string overrides or configuration
	/// settings. The format is a semicolon-delimited string (e.g., "Feature1;Feature2"). Frontend features may include
	/// the "FE_" prefix in this list.
	/// </remarks>
	/// <example>
	/// Example value: "FE_LegacyUI;DeprecatedFeature"
	/// </example>
	string BlockedFeatures { get; }

	/// <summary>
	/// Gets all the feature gates and their values.
	/// </summary>
	/// <remarks>
	/// This method retrieves all configured feature gates, including both static features from configuration and any
	/// overrides from query-string parameters. Frontend features (those prefixed with "FE_") will have their prefix
	/// removed in the returned dictionary. The method also applies any explicit enable/disable overrides from the
	/// RequestedFeatures and BlockedFeatures lists.
	///
	/// The returned dictionary uses case-insensitive keys to prevent duplicate features with different casing. Values
	/// can be Boolean (true/false) or other object types depending on the feature configuration.
	/// </remarks>
	/// <returns>
	/// A dictionary mapping feature-gate names (without "FE_" prefix for frontend features) to their values. The
	/// dictionary will be empty if no features are configured. Keys are case-insensitive.
	/// </returns>
	/// <example>
	/// Returned dictionary might contain:
	/// {
	///   "NewUI": true,           // Originally "FE_NewUI"
	///   "BetaFeature": false,    // Originally "FE_BetaFeature"
	///   "BackendCache": true     // Backend feature, no prefix
	/// }
	/// </example>
	Task<IDictionary<string, object>> GetFeatureGatesAsync();

	/// <summary>
	/// Gets the experiments that apply to the customer based on the provided filters.
	/// </summary>
	/// <remarks>
	/// This method queries the experiment manager to determine which experimental features or treatments apply to a
	/// customer based on the provided filters. The filters are used to evaluate experiment allocation rules and
	/// determine which experiments the customer should be enrolled in.
	/// </remarks>
	/// <param name="filters">The experiment filters to apply.</param>
	/// <param name="cancellationToken">The cancellation token for gracefully cancelling long-running asynchronous operations.</param>
	/// <returns>
	/// A dictionary mapping experiment/feature names to their values. Values can be:
	/// - Boolean: true/false indicating feature enabled/disabled
	/// - String: Configuration values or treatment identifiers
	/// - Other object types: Complex configuration objects
	/// Returns an empty dictionary if no experiments apply or if the experiment manager returns no results.
	/// </returns>
	/// <example>
	/// var filters = new Dictionary&lt;string, object&gt;
	/// {
	///     { "CustomerId", "12345" },
	///     { "Market", "US" },
	///     { "Platform", "Web" }
	/// };
	/// var experiments = await GetExperimentalFeaturesAsync(filters, cancellationToken);
	/// // Returns: { "NewCheckout": true, "Theme": "dark", "MaxItems": 100 }
	/// </example>
	Task<IDictionary<string, object>> GetExperimentalFeaturesAsync(IDictionary<string, string> filters, CancellationToken cancellationToken);

	/// <summary>
	/// Gets the feature-gate value associated with the assigned experiment flight.
	/// </summary>
	/// <remarks>
	/// This method retrieves the value of a specific experimental feature gate for a customer based on the provided
	/// filters. It internally calls GetExperimentalFeaturesAsync and extracts the value for the specified feature gate.
	///
	/// In most cases, experiment flights will be configured with Boolean values (true/false). However, string values
	/// can also be used as a way of sending configuration parameters to the service code for the flight. When a
	/// non-Boolean string value is returned, it indicates the customer is in a treatment group with that specific
	/// configuration.
	///
	/// The method interprets values as follows:
	/// - Missing or empty values: Customer not in experiment (returns InTreatment=false)
	/// - "true"/"false" strings or Booleans: Parsed as Boolean (returns InTreatment=true/false)
	/// - Other string values: Customer in treatment with configuration (returns InTreatment=true, Value=string)
	/// </remarks>
	/// <param name="featureGate">The name of the feature gate to check.</param>
	/// <param name="filters">The experiment filters to apply for determining allocation.</param>
	/// <param name="cancellationToken">The cancellation token for gracefully cancelling long-running asynchronous operations.</param>
	/// <returns>
	/// A FeatureGateResult containing:
	/// - InTreatment: true if the customer is allocated to a treatment (including when non-Boolean values are returned)
	/// - Value: The string value if non-Boolean, null otherwise
	/// </returns>
	/// <exception cref="ArgumentException">Thrown when featureGate is empty or whitespace.</exception>
	/// <example>
	/// // Check if customer is in new checkout experiment
	/// var result = await GetExperimentFeatureValueAsync("NewCheckout", filters, cancellationToken);
	/// if (result.InTreatment)
	/// {
	///     if (result.Value != null)
	///     {
	///         // Customer is in treatment with specific configuration
	///         var config = result.Value; // e.g., "variant_a" or "high_performance"
	///     }
	///     else
	///     {
	///         // Customer is in standard treatment (Boolean true)
	///     }
	/// }
	/// </example>
	Task<FeatureGateResult> GetExperimentFeatureValueAsync(string featureGate, IDictionary<string, string> filters, CancellationToken cancellationToken);

	/// <summary>
	/// Checks if the feature gate is active based on static configuration.
	/// </summary>
	/// <remarks>
	/// This method checks whether a feature gate is enabled in the feature manager's configuration, typically from
	/// appsettings.json or other configuration sources. It does not consider experiments or customer-specific
	/// targeting. It only checks the static feature-flag configuration.
	///
	/// This is useful for features that should be globally enabled/disabled regardless of customer context, or as a
	/// fallback when experiment allocation fails or returns no result.
	/// </remarks>
	/// <param name="featureGate">The name of the feature gate to check. Can include or omit the "FE_" prefix for frontend features.</param>
	/// <returns><c>true</c> if the feature gate is enabled in configuration; <c>false</c> otherwise.</returns>
	/// <exception cref="ArgumentException">Thrown when featureGate is empty or whitespace.</exception>
	/// <example>
	/// // Check if a feature is globally enabled
	/// bool isEnabled = await IsFeatureGateApplicableAsync("FE_NewDashboard");
	/// if (isEnabled)
	/// {
	///     // Feature is enabled for all customers
	/// }
	/// </example>
	Task<bool> IsFeatureGateApplicableAsync(string featureGate);

	/// <summary>
	/// Checks if the customer has been assigned to the specified experiment or if the feature is enabled.
	/// </summary>
	/// <remarks>
	/// This method provides a comprehensive check for feature availability by evaluating in the following order:
	///
	/// 1. Query-string overrides (via IExtendedFeatureManager.GetOverride) - Highest priority
	/// 2. Experiment allocation based on filters - Returns true for any non-empty, non-false value
	/// 3. Static feature configuration (via IsFeatureGateApplicableAsync) - Fallback
	///
	/// The method is designed to handle various experiment value types:
	/// - Boolean values: Directly indicate enabled/disabled
	/// - String "true"/"false": Parsed as Boolean
	/// - Other string values: Treated as treatment allocation (returns true)
	/// - Empty/whitespace values: Falls back to static configuration
	///
	/// This cascading evaluation ensures features can be controlled at multiple levels, from temporary overrides for
	/// testing to experiment-based rollouts to global configuration.
	/// </remarks>
	/// <param name="featureGate">The name of the feature gate to check.</param>
	/// <param name="filters">The experiment filters to apply for determining allocation.</param>
	/// <param name="cancellationToken">The cancellation token for gracefully cancelling long-running asynchronous operations.</param>
	/// <returns>
	/// <c>true</c> if the feature should be enabled for the customer based on:
	/// - Override configuration (highest priority)
	/// - Experiment allocation (including non-Boolean treatment values)
	/// - Static feature configuration (fallback)
	/// Returns <c>false</c> if none of the above enable the feature.
	/// </returns>
	/// <exception cref="ArgumentException">Thrown when featureGate is empty or whitespace.</exception>
	/// <example>
	/// // Check if customer should see new feature based on experiment or configuration
	/// var filters = new Dictionary&lt;string, object&gt; { { "CustomerId", "12345" } };
	/// bool showFeature = await IsExperimentApplicableAsync("NewFeature", filters, cancellationToken);
	///
	/// // The method will check:
	/// // 1. Query-string override (?features=NewFeature or ?blockedFeatures=NewFeature)
	/// // 2. Experiment allocation for customer 12345
	/// // 3. Static configuration in appsettings.json
	/// </example>
	Task<bool> IsExperimentApplicableAsync(string featureGate, IDictionary<string, string> filters, CancellationToken cancellationToken);
}
