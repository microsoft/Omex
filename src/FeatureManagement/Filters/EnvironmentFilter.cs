// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.FeatureManagement.Configuration;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;
using Microsoft.Omex.Extensions.FeatureManagement.Filters.Filter.Settings;

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters
{
	/// <summary>
	/// The environment filter.
	/// </summary>
	/// <param name="settings">The settings.</param>
	/// <param name="logger">The logger.</param>
	[FilterAlias("Environment")]
	internal sealed class EnvironmentFilter(
		IOptionsMonitor<ClusterSettings> settings,
		ILogger<EnvironmentFilter> logger) : IFeatureFilter
	{
		/// <inheritdoc/>
		public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
		{
			bool isEnabled = context.Evaluate<EnvironmentFilterSettings>(settings.CurrentValue.Environment, s => s.Environments);
			logger.LogInformation(Tag.Create(), $"{nameof(EnvironmentFilter)} returning {{IsEnabled}} for '{{FeatureName}}'.", isEnabled, context.FeatureName);
			return Task.FromResult(isEnabled);
		}
	}
}
