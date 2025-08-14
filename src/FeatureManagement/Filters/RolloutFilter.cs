// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;
using Microsoft.Omex.Extensions.FeatureManagement.Filters.Filter.Settings;

/// <summary>
/// The rollout filter, which is used to manage the rollout percentage exposure of a feature.
/// </summary>
/// <param name="httpContextAccessor">The HTTP context accessor.</param>
/// <param name="logger">The logger.</param>
[FilterAlias("Rollout")]
public sealed class RolloutFilter(
	IHttpContextAccessor httpContextAccessor,
	ILogger<RolloutFilter> logger) : IFeatureFilter
{
	/// <inheritdoc/>
	public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
	{
		bool isEnabled = Evaluate(context);
		logger.LogInformation(Tag.Create(), $"{nameof(RolloutFilter)} returning {{IsEnabled}} for '{{FeatureName}}'.", isEnabled, context.FeatureName);
		return Task.FromResult(isEnabled);
	}

	private bool Evaluate(FeatureFilterEvaluationContext context)
	{
		RolloutFilterSettings filterSettings = context.Parameters.GetOrCreate<RolloutFilterSettings>();
		if (httpContextAccessor.HttpContext is null ||
			!httpContextAccessor.HttpContext.User.TryGetEntraId(out Guid entraId))
		{
			logger.LogInformation(Tag.Create(), $"{nameof(RolloutFilter)} could not fetch the Entra ID.");
			return false;
		}

		int rolloutDecider = Math.Abs(entraId.GetHashCode()) % 100;
		return rolloutDecider <= filterSettings.ExposurePercentage;
	}
}
