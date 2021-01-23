// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
	/// <summary>
	/// Extension to add Omex dependencies to ApplicationBuilder
	/// </summary>
	public static class ApplicationBuilderExtensions
	{
		/// <summary>
		/// Adds the default exception handling logic that will display a developer exception page in develop and a short message during deployment
		/// </summary>
		/// <param name="builder">Application builder</param>
		/// <param name="environment">Environment information</param>
		/// <param name="enableCorrelationHeaderBackwardCompatibility">Set it to true if the service needs to accept a request with old style correlation</param>
		public static IApplicationBuilder UseOmexMiddlewares(
			this IApplicationBuilder builder,
			IHostEnvironment environment,
			bool enableCorrelationHeaderBackwardCompatibility)
		{
			builder
				.UseOmexExceptionHandler(environment)
				.UseActivityEnrichmentMiddleware()
				.UseResponseHeadersMiddleware();

			if (enableCorrelationHeaderBackwardCompatibility)
			{
#pragma warning disable CS0618 // We need to add this middleware to accept old correlation header
				builder.UseObsoleteCorrelationHeadersMiddleware();
#pragma warning restore CS0618
			}

			return builder;
		}

		/// <summary>
		/// Add middleware to enrich request activity with Result, SubType and Metadata
		/// </summary>
		public static IApplicationBuilder UseActivityEnrichmentMiddleware(this IApplicationBuilder builder) =>
			builder.UseMiddleware<ActivityEnrichmentMiddleware>();

		/// <summary>
		/// Add middleware that adds Omex headers to responses, like MachineId and BuildVersion
		/// </summary>
		public static IApplicationBuilder UseResponseHeadersMiddleware(this IApplicationBuilder builder) =>
			builder.UseMiddleware<ResponseHeadersMiddleware>();

		/// <summary>
		/// Add middleware that adds Omex headers to responses, like MachineId and BuildVersion
		/// </summary>
		[Obsolete("Use it only if you need to communicate with services that use old correlation", false)]
		public static IApplicationBuilder UseObsoleteCorrelationHeadersMiddleware(this IApplicationBuilder builder) =>
			builder.UseMiddleware<ObsoleteCorrelationHeadersMiddleware>();

		/// <summary>
		/// Adds the default exception handling logic that will display a developer exception page in develop and a short message during deployment
		/// </summary>
		public static IApplicationBuilder UseOmexExceptionHandler(this IApplicationBuilder builder, IHostEnvironment environment)
		{
			if (environment.IsDevelopment())
			{
				builder.UseDeveloperExceptionPage();
			}
			else
			{
				builder.UseExceptionHandler(appError =>
				{
					appError.Run(context =>
					{
						context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
						context.Response.ContentType = "application/json";

						IExceptionHandlerFeature contextFeature = context.Features.Get<IExceptionHandlerFeature>();

						Dictionary<string, string> errorDict = new Dictionary<string, string>
						{
							{ "Message", "Internal Server Error" },
							{ "ErrorMessage", contextFeature?.Error?.Message ?? string.Empty },
							{ "RequestId", Activity.Current?.Id ?? context.TraceIdentifier },
							{ "Suggestion", "For local debugging, set ASPNETCORE_ENVIRONMENT environment variable to 'Development' and restart the application" }
						};

						return context.Response.WriteAsync(JsonSerializer.Serialize(errorDict));
					});
				});
			}

			return builder;
		}
	}
}
