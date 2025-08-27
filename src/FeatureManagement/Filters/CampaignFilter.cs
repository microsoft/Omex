// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.FeatureManagement.Constants;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;
using Microsoft.Omex.Extensions.FeatureManagement.Filters.Settings;

/// <summary>
/// The campaign filter, to manage the inclusion or exclusion of some or all campaigns from a rolled out feature.
/// </summary>
/// <param name="httpContextAccessor">The HTTP context accessor.</param>
/// <param name="logger">The logger.</param>
[FilterAlias("Campaign")]
public sealed class CampaignFilter(
	IHttpContextAccessor httpContextAccessor,
	ILogger<CampaignFilter> logger) : IFeatureFilter
{
	/// <inheritdoc/>
	public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
	{
		bool isExcluded = context.Evaluate<CampaignFilterSettings>(httpContextAccessor, RequestParameters.Query.Campaign, s => s.Disabled);
		if (isExcluded)
		{
			logger.LogInformation(Tag.Create(), $"{nameof(CampaignFilter)} returning false for '{{FeatureName}}' as campaign is excluded.", context.FeatureName);
			return Task.FromResult(false);
		}

		bool isIncluded = context.Evaluate<CampaignFilterSettings>(httpContextAccessor, RequestParameters.Query.Campaign, s => s.Enabled);
		logger.LogInformation(Tag.Create(), $"{nameof(CampaignFilter)} returning {{IsEnabled}} for '{{FeatureName}}' as campaign is included.", isIncluded, context.FeatureName);
		return Task.FromResult(isIncluded);
	}
}
