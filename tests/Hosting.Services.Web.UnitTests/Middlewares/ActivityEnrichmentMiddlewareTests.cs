// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hosting.Services.Web.UnitTests
{
	[TestClass]
	[TestCategory("MiddlewareTests")]
	public class ActivityEnrichmentMiddlewareTests
	{
		[TestMethod]
		[DataRow(99, ActivityResult.SystemError)]
		[DataRow(100, ActivityResult.Success)]
		[DataRow(101, ActivityResult.Success)]
		[DataRow(399, ActivityResult.Success)]
		[DataRow(400, ActivityResult.ExpectedError)]
		[DataRow(401, ActivityResult.ExpectedError)]
		[DataRow(499, ActivityResult.ExpectedError)]
		[DataRow(500, ActivityResult.SystemError)]
		public async Task InvokeAsync_SetsActivityProperties(int statusCode, ActivityResult result)
		{
			string scheme = "https";
			int localPort = 8080;

			Task RequestDelegate(HttpContext context)
			{
				context.Response.StatusCode = statusCode;
				context.Request.Scheme = scheme;
				context.Connection.LocalPort = localPort;
				return Task.CompletedTask;
			}

			HttpContext context = new DefaultHttpContext();
			IMiddleware middleware = new ActivityEnrichmentMiddleware();

			Activity activity = new Activity("TestName").Start();

			await middleware.InvokeAsync(context, RequestDelegate).ConfigureAwait(false);

			activity.Stop();

			activity.AssertResult(result);
			activity.AssertTag(ActivityTagKeys.SubType, $"{scheme}:{localPort}");
			activity.AssertTag(ActivityTagKeys.Metadata, statusCode.ToString());
		}

		[TestMethod]
		public async Task InvokeAsync_WithoutActivity_NotFailing()
		{
			static Task RequestDelegate(HttpContext context)
			{
				return Task.CompletedTask;
			}

			HttpContext context = new DefaultHttpContext();
			IMiddleware middleware = new ActivityEnrichmentMiddleware();
			await middleware.InvokeAsync(context, RequestDelegate).ConfigureAwait(false);
		}
	}
}
