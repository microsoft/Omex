// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.FeatureManagement.Constants;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;

/// <summary>
/// A consolidator of feature gate information from multiple sources including appsettings.json and an experiment manager.
/// </summary>
/// <remarks>
/// This implementation of IFeatureGatesConsolidator serves as the primary aggregation point for feature-gate
/// information in web applications. It combines:
///
/// 1. Static feature flags from configuration (appsettings.json)
/// 2. Dynamic experimental features based on customer targeting
/// 3. Query-string overrides for testing and debugging
///
/// Key responsibilities:
/// - Orchestrates calls to IFeatureGatesService for both static and dynamic features
/// - Extracts partner information from HTTP headers for experiment targeting
/// - Merges results with experimental features taking precedence
/// - Provides comprehensive logging for debugging and monitoring
///
/// HTTP Context Dependency:
/// This service requires an active HTTP context to function properly. It uses the context to:
/// - Extract partner information from headers (e.g., X-Partner, X-Platform)
/// - Provide request-specific context for experiment allocation
/// - Support header prefix customization
/// </remarks>
/// <param name="activitySource">The activity source for distributed tracing.</param>
/// <param name="featureGatesService">The feature gates service for retrieving static and experimental features.</param>
/// <param name="httpContextAccessor">The HTTP context accessor for accessing request-specific information.</param>
/// <param name="logger">The logger for diagnostic information and debugging.</param>
internal sealed class FeatureGatesConsolidator(
	ActivitySource activitySource,
	IFeatureGatesService featureGatesService,
	IHttpContextAccessor httpContextAccessor,
	ILogger<FeatureGatesConsolidator> logger
) : IFeatureGatesConsolidator
{
	/// <inheritdoc/>
	/// <remarks>
	/// 1. Validates that HttpContext is available (throws InvalidOperationException if null)
	/// 2. Calls GetExperimentalFeaturesAsync to retrieve customer-specific experimental features
	///    - Extracts partner information from HTTP headers using the specified prefix
	///    - Uses the provided filters for experiment allocation
	///    - Logs the experimental features retrieved (or warns if none)
	/// 3. Calls IFeatureGatesService.GetFeatureGatesAsync for static features
	/// 4. Merges results using MergeExperimentalFeatures helper method
	///    - Experimental features override static features with the same name
	/// 5. Logs the final consolidated feature gates at Information level
	/// </remarks>
	public async Task<IDictionary<string, object>> GetFeatureGatesAsync(
		IDictionary<string, object> filters,
		string? headerPrefix = null,
		string? defaultPlatform = null,
		CancellationToken cancellationToken = default)
	{
		if (httpContextAccessor.HttpContext is null)
		{
			throw new InvalidOperationException($"{nameof(HttpContext)} is null. Ensure {nameof(IHttpContextAccessor)} is properly configured.");
		}

		IDictionary<string, object> experimentalFeatures = await GetExperimentalFeaturesAsync(
			httpContextAccessor.HttpContext,
			filters,
			headerPrefix,
			defaultPlatform,
			cancellationToken);
		IDictionary<string, object> result = await featureGatesService.GetFeatureGatesAsync();
		MergeExperimentalFeatures(result, experimentalFeatures);

		logger.LogInformation(Tag.Create(), "Successfully retrieved feature gates: {FeatureGates}", FormatDictionary(result));
		return result;
	}

	private async Task<IDictionary<string, object>> GetExperimentalFeaturesAsync(
		HttpContext httpContext,
		IDictionary<string, object> filters,
		string? headerPrefix,
		string? defaultPlatform,
		CancellationToken cancellationToken)
	{
		using Activity? activity = activitySource.StartActivity(FeatureManagementActivityNames.FeatureGatesConsolidator.GetExperimentalFeaturesAsync)?
			.SetSubType(httpContext.GetPartnerInfo(headerPrefix, defaultPlatform))?
			.MarkAsSystemError();

		logger.LogInformation(Tag.Create(), "Requesting experimental features with filters: {Filters}", filters);
		IDictionary<string, object> experimentalFeatures = await featureGatesService.GetExperimentalFeaturesAsync(filters, cancellationToken);
		if (experimentalFeatures.Count == 0)
		{
			logger.LogWarning(Tag.Create(), "Experimental features returned no results. If unexpected, check the filters and experiment manager setup.");
			activity?.SetMetadata("No applicable experiments.");
		}
		else
		{
			string experimentalFeaturesString = FormatDictionary(experimentalFeatures);
			logger.LogInformation(Tag.Create(), "Retrieved experimental features: {ExperimentalFeatures}", experimentalFeaturesString);
			activity?.SetMetadata(experimentalFeaturesString);
		}

		activity?.MarkAsSuccess();
		return experimentalFeatures;
	}

	private static void MergeExperimentalFeatures(IDictionary<string, object> target, IDictionary<string, object> source)
	{
		// Experimental features take precedence over the existing feature gate values.
		foreach (KeyValuePair<string, object> feature in source)
		{
			target[feature.Key] = feature.Value;
		}
	}

	private static string FormatDictionary(IDictionary<string, object> experimentalFeatures) =>
		string.Join(";", experimentalFeatures.Select(kvp => $"{kvp.Key}={kvp.Value}"));
}
