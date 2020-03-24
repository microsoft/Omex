﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	/// <summary>
	/// Extension to add Omex dependencies to ApplicationBuilder
	/// </summary>
	public static class ApplicationBuilderExtensions
	{
		/// <summary>
		/// Adds the default exception heandling logic that will display developer expeption page in develop and short message during deployment
		/// </summary>
		/// <param name="builder">Application builder</param>
		/// <param name="acceptObsoleteCorrelationHeader">Set it to true if service need to accept request with old style correlation</param>
		public static IApplicationBuilder UseOmexMiddlewares(this IApplicationBuilder builder, bool acceptObsoleteCorrelationHeader)
		{
			builder
				.UseMiddleware<ActivityEnrichmentMiddleware>()
				.UseMiddleware<ResponseHeadersMiddleware>();

			if (acceptObsoleteCorrelationHeader)
			{
#pragma warning disable CS0618 // We need to add this middleware to accept old correlation header
				builder.UseMiddleware<ObsoleteCorrelationHeadersMiddleware>();
#pragma warning restore CS0618
			}

			return builder;
		}

		/// <summary>
		/// Adds the default exception heandling logic that will display developer expeption page in develop and short message during deployment
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
							{ "Suggestion", "For local debugging, set ASPNETCORE_ENVIRONMENT environment variable to 'Development' and restart the app" }
						};

						return context.Response.WriteAsync(JsonSerializer.Serialize(errorDict));
					});
				});
			}

			return builder;
		}
	}
}
