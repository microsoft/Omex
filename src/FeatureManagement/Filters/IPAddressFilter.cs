// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;
using Microsoft.Omex.Extensions.FeatureManagement.Filters.Configuration;
using Microsoft.Omex.Extensions.FeatureManagement.Filters.Settings;

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters
{
	/// <summary>
	/// The IP address filter.
	/// </summary>
	/// <param name="httpContextAccessor">The HTTP context accessor.</param>
	/// <param name="ipRangeProvider">The IP range provider.</param>
	/// <param name="logger">The logger.</param>
	[FilterAlias("IPAddress")]
	public sealed class IPAddressFilter(
		IHttpContextAccessor httpContextAccessor,
		IIPRangeProvider ipRangeProvider,
		ILogger<IPAddressFilter> logger) : IFeatureFilter
	{
		/// <inheritdoc/>
		public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
		{
			bool isEnabled = Evaluate(context);
			logger.LogInformation(Tag.Create(), $"{nameof(IPAddressFilter)} returning {{IsEnabled}} for '{{FeatureName}}'.", isEnabled, context.FeatureName);
			return Task.FromResult(isEnabled);
		}

		private bool Evaluate(FeatureFilterEvaluationContext context)
		{
			IPAddressFilterSettings filterSettings = context.Parameters.GetOrCreate<IPAddressFilterSettings>();
			if (string.IsNullOrWhiteSpace(filterSettings.AllowedRange) ||
				httpContextAccessor.HttpContext is null)
			{
				return false;
			}

			if (httpContextAccessor.HttpContext.IsLocal())
			{
				return true;
			}

			IPAddress remoteAddress = httpContextAccessor.HttpContext.GetForwardedAddress();
			bool isInRange = IsInIPRange(filterSettings.AllowedRange, remoteAddress);
			logger.LogInformation(Tag.Create(), $"{nameof(IPAddressFilter)} {{Verb}} access to '{{FeatureName}}'.", isInRange ? "allowed" : "blocked", context.FeatureName);
			return isInRange;
		}

		private bool IsInIPRange(string rangeName, IPAddress callerAddress)
		{
			IPNetwork[]? ipRanges = ipRangeProvider.GetIPRanges(rangeName);
			if (ipRanges is null)
			{
				return false;
			}

			return Array.Exists(ipRanges, network => network.Contains(callerAddress));
		}
	}
}
