// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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

namespace Microsoft.Omex.Extensions.FeatureManagement
{
	/// <summary>
	/// A consolidator of feature gate information from multiple sources including appsettings.json and an experiment manager.
	/// </summary>
	/// <param name="activitySource">The activity source.</param>
	/// <param name="featureGatesService">The feature gates service.</param>
	/// <param name="httpContextAccessor">The HTTP context accessor.</param>
	/// <param name="logger">The logger.</param>
	internal sealed class FeatureGatesConsolidator(
		ActivitySource activitySource,
		IFeatureGatesService featureGatesService,
		IHttpContextAccessor httpContextAccessor,
		ILogger<FeatureGatesConsolidator> logger
	) : IFeatureGatesConsolidator
	{
		/// <inheritdoc/>
		public async Task<IDictionary<string, object>> GetFeatureGatesAsync(
			IDictionary<string, object> filters,
			string? headerPrefix = null,
			string? defaultPlatform = null,
			CancellationToken cancellationToken = default)
		{
			if (httpContextAccessor.HttpContext == null)
			{
				throw new InvalidOperationException($"{nameof(HttpContext)} is null. Ensure {nameof(IHttpContextAccessor)} is properly configured.");
			}

			IDictionary<string, object>? experimentalFeatures = await GetExperimentalFeaturesAsync(
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

		private async Task<IDictionary<string, object>?> GetExperimentalFeaturesAsync(
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

		private static void MergeExperimentalFeatures(IDictionary<string, object> target, IDictionary<string, object>? source)
		{
			if (source == null)
			{
				return;
			}

			// Experimental features take precedence over the existing feature gate values.
			foreach (KeyValuePair<string, object> feature in source)
			{
				target[feature.Key] = feature.Value;
			}
		}

		private static string FormatDictionary(IDictionary<string, object> experimentalFeatures) =>
			string.Join(";", experimentalFeatures.Select(kvp => $"{kvp.Key}={kvp.Value}"));
	}
}
