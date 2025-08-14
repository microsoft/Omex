// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.Omex.Extensions.FeatureManagement.Extensions
{
	/// <summary>
	/// Extension methods for <see cref="ClaimsPrincipal"/>.
	/// </summary>
	internal static class ClaimsPrincipalExtensions
	{
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
}
