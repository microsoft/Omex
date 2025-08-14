// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;

namespace Microsoft.Omex.Extensions.FeatureManagement.Types
{
	/// <summary>
	/// Represents a customer ID with an associated type.
	/// </summary>
	/// <param name="Id">The customer ID.</param>
	/// <param name="Type">The type of the customer ID.</param>
	public readonly record struct CustomerId(string Id, string Type)
	{
		/// <summary>
		/// Creates a new instance of <see cref="CustomerId"/> using the current customer's Entra ID.
		/// </summary>
		/// <param name="httpContext">The HTTP context.</param>
		/// <returns>The constructed <see cref="CustomerId"/>.</returns>
		public static CustomerId FromEntraId(HttpContext httpContext)
		{
			if (!httpContext.User.IsAuthenticated() ||
				!httpContext.User.TryGetEntraId(out Guid entraId))
			{
				return new();
			}

			return new(entraId.ToString(), "OrgIdPuid");
		}
	}
}
