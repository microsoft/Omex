// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.FeatureManagement.Tests.TestUtilities;

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

/// <summary>
/// A factory for creating <see cref="HttpContextAccessor"/>s for unit testing.
/// </summary>
public static class HttpContextAccessorFactory
{
	/// <summary>
	/// Creates a new <see cref="HttpContextAccessor"/>.
	/// </summary>
	/// <returns>The <see cref="HttpContextAccessor"/>.</returns>
	public static HttpContextAccessor Create() =>
		Create([]);

	/// <summary>
	/// Creates a new <see cref="HttpContextAccessor"/>.
	/// </summary>
	/// <param name="queryParameters">The query-string parameters to add to the HTTP context.</param>
	/// <returns>The <see cref="HttpContextAccessor"/>.</returns>
	public static HttpContextAccessor Create(Dictionary<string, StringValues> queryParameters)
	{
		DefaultHttpContext httpContext = new();
		httpContext.Request.Query = new QueryCollection(queryParameters);

		return new()
		{
			HttpContext = httpContext,
		};
	}
}
