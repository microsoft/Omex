// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.FeatureManagement.Constants;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters
{
	/// <summary>
	/// The filter to allow a feature to be toggled to the enabled state while preserving the evaluation of other filters on the feature.
	/// </summary>
	/// <param name="httpContextAccessor">The HTTP context accessor.</param>
	/// <param name="logger">The logger.</param>
	/// <param name="settings">The settings.</param>
	[FilterAlias("Toggle")]
	internal sealed class ToggleFilter(
		IHttpContextAccessor httpContextAccessor,
		ILogger<ToggleFilter> logger,
		IOptionsMonitor<FeatureOverrideSettings> settings) : IFeatureFilter
	{
		/// <inheritdoc/>
		public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
		{
			bool isEnabled = Evaluate(context);
			logger.LogInformation(Tag.Create(), $"{nameof(ToggleFilter)} returning {{IsEnabled}} for '{{FeatureName}}'.", isEnabled, context.FeatureName);
			return Task.FromResult(isEnabled);
		}

		private bool Evaluate(FeatureFilterEvaluationContext context)
		{
			string toggledFeatures = httpContextAccessor.GetParameter(RequestParameters.Query.ToggledFeatures);
			string[] toggledFeaturesList = toggledFeatures.Split(';', StringSplitOptions.RemoveEmptyEntries);
			return toggledFeaturesList.Contains(context.FeatureName, StringComparer.OrdinalIgnoreCase) ||
				Array.Exists(settings.CurrentValue.Toggled, f => string.Equals(f, context.FeatureName, StringComparison.OrdinalIgnoreCase));
		}
	}
}
