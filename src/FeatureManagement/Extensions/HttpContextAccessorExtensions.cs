// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Omex.Extensions.FeatureManagement.Constants;

namespace Microsoft.Omex.Extensions.FeatureManagement.Extensions
{
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

		/// <summary>
		/// Gets the correlation ID from the request context.
		/// </summary>
		/// <param name="contextAccessor">The HTTP request context accessor.</param>
		/// <returns>The correlation ID, or a random GUID if the correlation ID cannot be retrieved.</returns>
		public static Guid GetCorrelationId(this IHttpContextAccessor contextAccessor)
		{
			string parameterValue = contextAccessor.GetParameter(RequestParameters.Query.CorrelationId);
			if (Guid.TryParse(parameterValue, out Guid correlationId))
			{
				return correlationId;
			}

			// If the correlation ID is not present or invalid, return the empty GUID. This was chosen to ensure the
			// behavior is deterministic, as generating a random GUID would result in different values on each call.
			return Guid.Empty;
		}

		/// <summary>
		/// Gets the language from the request context.
		/// </summary>
		/// <param name="contextAccessor">The HTTP request context accessor.</param>
		/// <returns>The language, or <see cref="CultureInfo.InvariantCulture"/> if the language cannot be retrieved.</returns>
		public static CultureInfo GetLanguage(this IHttpContextAccessor contextAccessor)
		{
			string language = contextAccessor.GetParameter(RequestParameters.Query.Language);

			try
			{
				return CultureInfo.GetCultureInfo(language);
			}
			catch (CultureNotFoundException)
			{
				return CultureInfo.InvariantCulture;
			}
		}
	}
}
