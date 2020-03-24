// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	/// <summary>
	/// Enrich request activity with Result, SubType and Metadata
	/// </summary>
	public class ActivityEnrichmentMiddleware
	{
		internal ActivityEnrichmentMiddleware(RequestDelegate next) => m_next = next;

		/// <summary>
		/// Invoke middleware
		/// </summary>
		public async Task InvokeAsync(HttpContext context)
		{
			await m_next(context).ConfigureAwait(false);

			Activity? activity = Activity.Current;

			if (activity == null)
			{
				return;
			}

			int statusCode = context.Response.StatusCode;

			activity.SetResult(GetResult(statusCode))
				.SetSubType($"{context.Request.Scheme}:{context.Connection.LocalPort}")
				.SetMetadata(statusCode.ToString());
		}

		private TimedScopeResult GetResult(int? statusCode) =>
			statusCode >= 100 && statusCode < 400
				? TimedScopeResult.Success
				: statusCode >= 400 && statusCode < 500
					? TimedScopeResult.ExpectedError
					: TimedScopeResult.SystemError;

		private readonly RequestDelegate m_next;
	}
}
