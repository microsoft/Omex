// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.Abstractions;

/// <summary>
/// The filter to allow a feature to be toggled to the enabled state while preserving the evaluation of other filters on the feature.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="settings">The settings.</param>
[FilterAlias("Toggle")]
public sealed class ToggleFilter(
	ILogger<ToggleFilter> logger,
	IOptionsMonitor<FeatureOverrideSettings> settings) : IFeatureFilter
{
	/// <inheritdoc/>
	public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
	{
		bool isEnabled = Evaluate(context);
		logger.LogInformation(Tag.Create(), $"{nameof(ToggleFilter)} returning '{{IsEnabled}}' for '{{FeatureName}}'.", isEnabled, context.FeatureName);
		return Task.FromResult(isEnabled);
	}

	private bool Evaluate(FeatureFilterEvaluationContext context)
		=> Array.Exists(settings.CurrentValue.Toggled, feature =>
			string.Equals(feature, context.FeatureName, StringComparison.OrdinalIgnoreCase));
}
