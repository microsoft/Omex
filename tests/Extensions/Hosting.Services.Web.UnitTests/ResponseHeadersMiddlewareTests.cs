﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hosting.Services.Web.UnitTests
{
	[TestClass]
	public class ResponseHeadersMiddlewareTests
	{
		[TestMethod]
		[TestCategory("MiddlewareTests")]
		public async Task InvokeAsync_WithoutActivity_NotFailing()
		{
			string machineId = "MachineIdTestValue";
			string buildVersion = "BuildVersionTestValue";

			Mock<IExecutionContext> mock = new Mock<IExecutionContext>();
			mock.SetupGet(c => c.MachineId).Returns(machineId);
			mock.SetupGet(c => c.BuildVersion).Returns(buildVersion);

			ResponseFeatureForCallbacks feature = new ResponseFeatureForCallbacks();
			HttpContext context = new DefaultHttpContext();
			context.Features.Set<IHttpResponseFeature>(feature);
			IMiddleware middleware = new ResponseHeadersMiddleware(mock.Object);

			await middleware.InvokeAsync(context, s => Task.CompletedTask).ConfigureAwait(false);
			await feature.InvokeOnStarting().ConfigureAwait(false);

			mock.Verify(c => c.MachineId, Times.Once);
			mock.Verify(c => c.BuildVersion, Times.Once);

			Assert.AreEqual(machineId, context.Response.Headers["X-Machine"].FirstOrDefault());
			Assert.AreEqual(buildVersion, context.Response.Headers["X-BuildVersion"].FirstOrDefault());
		}
	}
}
