// Copyright (C) Microsoft Corporation. All rights reserved.
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
using Microsoft.Omex.Extensions.FeatureManagement.Experimentation;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;
using Microsoft.Omex.Extensions.FeatureManagement.Types;

namespace Microsoft.Omex.Extensions.FeatureManagement
{
	/// <summary>
	/// A consolidator of feature gate information from multiple sources including appsettings.json and an experimentation manager.
	/// </summary>
	/// <param name="activitySource">The activity source.</param>
	/// <param name="experimentManager">The experiment manager.</param>
	/// <param name="featureGatesService">The feature gates service.</param>
	/// <param name="httpContextAccessor">The HTTP context accessor.</param>
	/// <param name="logger">The logger.</param>
	internal sealed class FeatureGatesConsolidator(
		ActivitySource activitySource,
		IExperimentManager experimentManager,
		IFeatureGatesService featureGatesService,
		IHttpContextAccessor httpContextAccessor,
		ILogger<FeatureGatesConsolidator> logger
	) : IFeatureGatesConsolidator
	{
		/// <inheritdoc/>
		public async Task<IDictionary<string, bool>> GetFeatureGatesForEntraIdAsync(
			string? headerPrefix = null,
			string? defaultPlatform = null,
			CancellationToken cancellationToken = default)
		{
			if (httpContextAccessor.HttpContext == null)
			{
				throw new InvalidOperationException($"{nameof(HttpContext)} is null. Ensure {nameof(IHttpContextAccessor)} is properly configured.");
			}

			CustomerId customerId = CustomerId.FromEntraId(httpContextAccessor.HttpContext);
			if (string.IsNullOrWhiteSpace(customerId.Id))
			{
				logger.LogWarning(Tag.Create(), "Entra ID could not be retrieved. Customer is likely not signed in via an Entra ID.");
			}

			return await GetFeatureGatesAsync(customerId, headerPrefix, defaultPlatform, cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<IDictionary<string, bool>> GetFeatureGatesAsync(
			CustomerId? customerId = null,
			string? headerPrefix = null,
			string? defaultPlatform = null,
			CancellationToken cancellationToken = default)
		{
			if (httpContextAccessor.HttpContext == null)
			{
				throw new InvalidOperationException($"{nameof(HttpContext)} is null. Ensure {nameof(IHttpContextAccessor)} is properly configured.");
			}

			ExperimentFilters filters = CreateExperimentFilters(httpContextAccessor.HttpContext, customerId, headerPrefix, defaultPlatform);
			IDictionary<string, bool> experimentalFeatures = await GetExperimentalFeaturesAsync(
				httpContextAccessor.HttpContext,
				filters,
				headerPrefix,
				defaultPlatform,
				cancellationToken);
			IDictionary<string, bool> result = await featureGatesService.GetFeatureGatesAsync();
			MergeExperimentalFeatures(result, experimentalFeatures);

			logger.LogInformation(Tag.Create(), "Successfully retrieved feature gates: {FeatureGates}", FormatDictionary(result));
			return result;
		}

		private ExperimentFilters CreateExperimentFilters(HttpContext httpContext, CustomerId? customerId, string? headerPrefix, string? defaultPlatform) =>
			new()
			{
				Browser = httpContext.GetBrowser(),
				Campaign = httpContextAccessor.GetParameter(RequestParameters.Query.Campaign),
				CorrelationId = httpContextAccessor.GetCorrelationId(),
				CustomerId = customerId ?? new(),
				DeviceType = httpContext.GetDeviceType().ToString(),
				Language = httpContextAccessor.GetLanguage(),
				Market = httpContextAccessor.GetParameter(RequestParameters.Query.Market),
				Platform = httpContext.GetPlatform(headerPrefix, defaultPlatform),
			};

		private async Task<IDictionary<string, bool>> GetExperimentalFeaturesAsync(
			HttpContext httpContext,
			ExperimentFilters filters,
			string? headerPrefix,
			string? defaultPlatform,
			CancellationToken cancellationToken)
		{
			using Activity? activity = activitySource.StartActivity(FeatureManagementActivityNames.FeatureGatesConsolidator.GetExperimentalFeaturesAsync)?
				.SetSubType(httpContext.GetPartnerInfo(headerPrefix, defaultPlatform))?
				.MarkAsSystemError();

			logger.LogInformation(Tag.Create(), "Requesting experimental features with filters: {Filters}", filters);
			IDictionary<string, bool> experimentalFeatures = await experimentManager.GetExperimentStatusesAsync(filters, cancellationToken);
			if (experimentalFeatures.Count == 0)
			{
				logger.LogWarning(Tag.Create(), "Experimental features returned no results. If unexpected, check the filters and ECS setup.");
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

		private static void MergeExperimentalFeatures(IDictionary<string, bool> target, IDictionary<string, bool> source)
		{
			if (source == null)
			{
				return;
			}

			// Experimental features take precedence over the existing feature gate values.
			foreach (KeyValuePair<string, bool> feature in source)
			{
				target[feature.Key] = feature.Value;
			}
		}

		private static string FormatDictionary(IDictionary<string, bool> experimentalFeatures) =>
			string.Join(";", experimentalFeatures.Select(kvp => $"{kvp.Key}={kvp.Value}"));
	}
}
