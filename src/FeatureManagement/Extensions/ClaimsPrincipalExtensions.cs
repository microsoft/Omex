// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.Extensions.FeatureManagement.Extensions;

using System;
using System.Linq;
using System.Security.Claims;

/// <summary>
/// Extension methods for <see cref="ClaimsPrincipal"/>.
/// </summary>
public static class ClaimsPrincipalExtensions
{
	/// <summary>
	/// Determines whether the specified <see cref="ClaimsPrincipal"/> is authenticated.
	/// </summary>
	/// <param name="claimsPrincipal">The object to check for authentication.</param>
	/// <returns><see langword="true"/> if the customer is authenticated; <see langword="false"/> otherwise.</returns>
	public static bool IsAuthenticated(this ClaimsPrincipal claimsPrincipal) =>
		claimsPrincipal.Identity?.IsAuthenticated ?? false;

	/// <summary>
	/// Tries to get a customer's Entra ID from their token claims.
	/// </summary>
	/// <param name="claimsPrincipal">The object from which to retrieve the Entra ID.</param>
	/// <param name="entraId">The Entra ID as a <see cref="Guid"/>.</param>
	/// <returns>A value indicating whether the Entra ID could be retrieved.</returns>
	public static bool TryGetEntraId(this ClaimsPrincipal claimsPrincipal, out Guid entraId)
	{
		string? entraIdString = claimsPrincipal.Claims
			.FirstOrDefault(c => string.Equals(c.Type, "oid", StringComparison.OrdinalIgnoreCase))?
			.Value;
		return Guid.TryParse(entraIdString, out entraId);
	}
}
