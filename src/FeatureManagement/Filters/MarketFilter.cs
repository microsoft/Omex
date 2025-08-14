// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.FeatureManagement.Constants;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;
using Microsoft.Omex.Extensions.FeatureManagement.Filters.Settings;

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters
{
	/// <summary>
	/// The market filter.
	/// </summary>
	/// <param name="httpContextAccessor">The HTTP context accessor.</param>
	/// <param name="logger">The logger.</param>
	[FilterAlias("Market")]
	public sealed class MarketFilter(
		IHttpContextAccessor httpContextAccessor,
		ILogger<MarketFilter> logger) : IFeatureFilter
	{
		/// <inheritdoc/>
		public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
		{
			bool isExcluded = context.Evaluate<MarketFilterSettings>(httpContextAccessor, RequestParameters.Query.Market, s => s.Disabled);
			if (isExcluded)
			{
				logger.LogInformation(Tag.Create(), $"{nameof(MarketFilter)} returning false for '{{FeatureName}}' as market is excluded.", context.FeatureName);
				return Task.FromResult(false);
			}

			bool isIncluded = context.Evaluate<MarketFilterSettings>(httpContextAccessor, RequestParameters.Query.Market, s => s.Enabled);
			logger.LogInformation(Tag.Create(), $"{nameof(MarketFilter)} returning {{IsEnabled}} for '{{FeatureName}}' as market is included.", isIncluded, context.FeatureName);
			return Task.FromResult(isIncluded);
		}
	}
}
