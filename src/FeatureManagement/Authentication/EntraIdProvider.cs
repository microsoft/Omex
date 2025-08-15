// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Authentication;

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;

/// <summary>
/// Provides the Entra ID as the customer ID.
/// </summary>
/// <param name="httpContextAccessor">The HTTP context accessor.</param>
/// <param name="logger">The logger.</param>
internal sealed class EntraIdProvider(
	IHttpContextAccessor httpContextAccessor,
	ILogger<EntraIdProvider> logger) : ICustomerIdProvider
{
	/// <inheritdoc />
	public string GetCustomerId()
	{
		if (httpContextAccessor.HttpContext is null ||
			!httpContextAccessor.HttpContext.User.TryGetEntraId(out Guid entraId))
		{
			logger.LogInformation(Tag.Create(), $"{nameof(EntraIdProvider)} could not fetch the Entra ID.");
			return string.Empty;
		}

		return entraId.ToString();
	}
}
