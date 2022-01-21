// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	/// <summary>
	/// Adds support of old correlation tracking logic that might be used by other services
	/// </summary>
	/// <remarks>
	/// This middleware should be removed after all services moved to use <see cref="Activity"/>
	/// </remarks>
	[Obsolete("Use it only if you need to communicate with services that use old correlation", false)]
	internal class ObsoleteCorrelationHeadersMiddleware : IMiddleware
	{
		Task IMiddleware.InvokeAsync(HttpContext context, RequestDelegate next)
		{
			ExtractCorrelationFromRequest(context.Request);
			return next(context);
		}

		private void ExtractCorrelationFromRequest(HttpRequest request)
		{
			Activity? activity = Activity.Current;
			if (activity == null)
			{
				return;
			}

			Guid? oldCorrelation = ExtractCorrelationIdFromHeader(request) ?? ExtractCorrelationIdFromQuery(request);
			if (oldCorrelation.HasValue)
			{
				activity.SetObsoleteCorrelationId(oldCorrelation.Value);
			}

			IQueryCollection dataSources = request.Query;

			uint? transactionId = ParseUint(ExtractParameter(dataSources, s_transactionsNames));
			if (transactionId.HasValue)
			{
				activity.SetObsoleteTransactionId(transactionId.Value);
			}
		}

		private static Guid? ExtractCorrelationIdFromQuery(HttpRequest request) =>
			!IsClientRequest(request)
			&& Guid.TryParse(ExtractParameter(request.Query, s_correlationIdNames), out Guid correlation)
				? correlation
				: null;

		private static Guid? ExtractCorrelationIdFromHeader(HttpRequest request) =>
			!IsClientRequest(request)
			&& Guid.TryParse(ExtractHeader(request.Headers, s_correlationIdNames), out Guid correlation)
				? correlation
				: null;

		private static string? ExtractParameter(IQueryCollection dataSources, IEnumerable<string> names)
		{
			foreach (string name in names)
			{
				string? value = ExtractParameter(dataSources, name);
				if (!string.IsNullOrWhiteSpace(value))
				{
					return value;
				}
			}

			return null;
		}

		private static string? ExtractParameter(IQueryCollection dataSources, string name) =>
			dataSources.TryGetValue(name, out StringValues value) && value.Count > 0
				? value.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))
				: null;

		private static string? ExtractHeader(IHeaderDictionary headers, IEnumerable<string> names)
		{
			foreach (string name in names)
			{
				string? value = headers[name];
				if (!string.IsNullOrWhiteSpace(value))
				{
					return value;
				}
			}

			return null;
		}

		/// <summary>
		/// Checks if the context is for a request that contains identifiers indicating that the request originated from an Office client
		/// </summary>
		private static bool IsClientRequest(HttpRequest request) =>
			request.Headers.ContainsKey(s_officeClientVersionHeader)
				? true
				: request.Query.Count == 0 // Don't check empty parameters
					? false
					: s_officeClientQueryParameters.Any(p => request.Query.ContainsKey(p)); // Check if the request contains an Office client query parameter

		private static uint? ParseUint(string? value) =>
			uint.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out uint result)
				? result
				: null;

		private static readonly string s_correlationHeader = "X-CorrelationId";

		private static readonly string s_officeClientVersionHeader = "X-Office-Version";

		private static readonly string[] s_transactionsNames = {
			"corrtid",					// Correlation transaction query parameter
			"X-TransactionId" };

		private static readonly string[] s_correlationIdNames = {
			s_correlationHeader,
			"MS-CorrelationId",			// Correlation Id header used by other Microsoft services
			"corr",						// Correlation query parameter
			"HTTP_X_CORRELATIONID" };

		private static readonly string[] s_officeClientQueryParameters = {
			"client",					// Identifies the client application and platform
			"av",						// Identifies the client application, platform and partial version
			"app" };					// Identifies the client application
	}
}
