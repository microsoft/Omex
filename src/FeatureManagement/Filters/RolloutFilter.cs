// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.FeatureManagement.Authentication;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;
using Microsoft.Omex.Extensions.FeatureManagement.Filters.Settings;

/// <summary>
/// The rollout filter, which is used to manage the rollout percentage exposure of a feature based on the customer ID.
/// </summary>
/// <param name="customerIdProvider">The customer ID provider.</param>
/// <param name="logger">The logger.</param>
[FilterAlias("Rollout")]
public sealed class RolloutFilter(
	ICustomerIdProvider customerIdProvider,
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
		string customerId = customerIdProvider.GetCustomerId();
		int rolloutDecider = Math.Abs(customerId.GetHashCode()) % 100;
		return rolloutDecider <= filterSettings.ExposurePercentage;
	}
}
