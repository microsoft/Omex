﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Omex.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	/// <summary>
	/// Adds Omex headers to responses, like MachineId and BuildVersion
	/// </summary>
	internal class ResponseHeadersMiddleware : IMiddleware
	{
		public ResponseHeadersMiddleware(IExecutionContext context) => m_context = context;

		Task IMiddleware.InvokeAsync(HttpContext context, RequestDelegate next)
		{
			context.Response.OnStarting(SetResponseHeaders, context.Response);
			return next(context);
		}

		private Task SetResponseHeaders(object state)
		{
			HttpResponse response = (HttpResponse)state;
			response.Headers.Add("X-Machine", m_context.MachineId);
			response.Headers.Add("X-BuildVersion", m_context.BuildVersion); //Renamed from X-OfficeVersion
			return Task.CompletedTask;
		}

		private readonly IExecutionContext m_context;
	}
}
