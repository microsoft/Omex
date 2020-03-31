// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.Omex.Extensions.TimedScopes.UnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hosting.Services.Web.UnitTests
{
	[TestClass]
	[TestCategory("MiddlewareTests")]
	public class ActivityEnrichmentMiddlewareTests
	{
		[DataTestMethod]
		[DataRow(99, TimedScopeResult.SystemError)]
		[DataRow(100, TimedScopeResult.Success)]
		[DataRow(101, TimedScopeResult.Success)]
		[DataRow(399, TimedScopeResult.Success)]
		[DataRow(400, TimedScopeResult.ExpectedError)]
		[DataRow(401, TimedScopeResult.ExpectedError)]
		[DataRow(499, TimedScopeResult.ExpectedError)]
		[DataRow(500, TimedScopeResult.SystemError)]
		public async Task InvokeAsync_SetsActivityProperties(int statusCode, TimedScopeResult result)
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

			await middleware.InvokeAsync(context, RequestDelegate);

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
			await middleware.InvokeAsync(context, RequestDelegate);
		}
	}
}
