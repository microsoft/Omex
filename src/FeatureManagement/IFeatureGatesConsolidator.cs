// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

/// <summary>
/// A consolidator of feature gate information from multiple sources including appsettings.json and an experiment manager.
/// </summary>
/// <remarks>
/// This service acts as a unified entry point for retrieving feature gate values, consolidating information
/// from multiple sources in a prioritized manner. It combines:
///
/// 1. Static feature flags from configuration (appsettings.json)
/// 2. Dynamic experimental features based on customer targeting
/// 3. Query-string overrides for testing and debugging
///
/// The consolidator requires an active HTTP context to operate, as it extracts partner information and other
/// request-specific data from HTTP headers. Experimental features take precedence over static configuration when
/// merging results.
///
/// This service is typically used in web applications where you need to provide a complete set of feature flags to a
/// client application, combining both server-side configuration and customer-specific experimental treatments.
/// </remarks>
public interface IFeatureGatesConsolidator
{
	/// <summary>
	/// Gets the consolidated mapping of feature-gate names to their values from all sources.
	/// </summary>
	/// <remarks>
	/// This method performs the following operations:
	///
	/// 1. Retrieves experimental features using the provided filters and HTTP context information
	/// 2. Retrieves static feature gates from the IFeatureGatesService
	/// 3. Merges the results, with experimental features taking precedence over static ones
	/// 4. Logs the consolidated results for debugging and monitoring purposes
	///
	/// The method requires an active HTTP context to extract partner information from headers. If the HTTP context is
	/// unavailable, an InvalidOperationException will be thrown.
	///
	/// Frontend features (those prefixed with "FE_") will have their prefix removed in the returned dictionary, making
	/// them ready for client consumption.
	///
	/// The consolidation process ensures that:
	/// - Customers in experiments get their assigned treatments
	/// - Customers not in experiments fall back to static configuration
	/// - Override settings (from query strings) are respected
	/// </remarks>
	/// <param name="filters"> The experiment filters to apply when retrieving experimental features.</param>
	/// <param name="headerPrefix">
	/// Optional prefix for HTTP headers when extracting partner information. If provided, headers like
	/// "{prefix}-Partner" will be checked for partner data. If null or empty, the default header names will be used.
	/// This is useful when you have multiple services with different header naming conventions.
	/// </param>
	/// <param name="defaultPlatform">The default platform value to use if not specified in the HTTP headers or filters.</param>
	/// <param name="cancellationToken">The cancellation token for gracefully cancelling long-running asynchronous operations.</param>
	/// <returns>
	/// A dictionary mapping feature-gate names to their consolidated values. Keys are feature names (without "FE_"
	/// prefix for frontend features).
	/// Values can be:
	/// - Boolean: true/false for simple feature flags
	/// - String: Configuration values or treatment identifiers
	/// - Other object types: Complex configuration objects
	///
	/// The returned dictionary will contain all applicable features from both static configuration and experiments. An
	/// empty dictionary is returned if no features are configured or applicable.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown when <see cref="HttpContext"/> is <c>null</c>, indicating that <see cref="IHttpContextAccessor"/> is not
	/// properly configured or the method is being called outside of an HTTP request context.
	/// </exception>
	/// <example>
	/// // In a controller or middleware
	/// var filters = new Dictionary&lt;string, object&gt;
	/// {
	///     { "CustomerId", Customer.GetCustomerId() },
	///     { "Market", Customer.GetMarket() }
	/// };
	///
	/// var features = await consolidator.GetFeatureGatesAsync(
	///     filters,
	///     headerPrefix: "X-MyApp",
	///     defaultPlatform: "Web",
	///     cancellationToken);
	///
	/// // Returns consolidated features like:
	/// // {
	/// //   "NewUI": true,           // From experiment
	/// //   "BetaFeature": false,    // From static config
	/// //   "Theme": "dark",         // From experiment with config value
	/// //   "MaxItems": 100          // From static config
	/// // }
	/// </example>
	Task<IDictionary<string, object>> GetFeatureGatesAsync(
		IDictionary<string, object> filters,
		string? headerPrefix = null,
		string? defaultPlatform = null,
		CancellationToken cancellationToken = default);
}
