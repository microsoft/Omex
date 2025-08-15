// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Extensions;

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

/// <summary>
/// Extension methods for <see cref="IHttpContextAccessor"/>.
/// </summary>
internal static class HttpContextAccessorExtensions
{
	/// <summary>
	/// Retrieves the values of the named parameter from the request context.
	/// </summary>
	/// <param name="contextAccessor">The HTTP request context accessor.</param>
	/// <param name="paramName">The name of the parameter.</param>
	/// <returns>The parameter values if the parameter is present, or <see cref="StringValues.Empty"/> if it is not.</returns>
	public static string GetParameter(this IHttpContextAccessor contextAccessor, string paramName)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(paramName);

		HttpRequest? request = contextAccessor.HttpContext?.Request;
		if (request is null)
		{
			return string.Empty;
		}

		if (request.Query.TryGetValue(paramName, out StringValues values) ||
			request.Headers.TryGetValue(paramName, out values))
		{
			return values.ToString();
		}

		return string.Empty;
	}
}
