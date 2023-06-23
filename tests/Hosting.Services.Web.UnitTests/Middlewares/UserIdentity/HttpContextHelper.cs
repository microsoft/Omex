// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests
{
	internal class HttpContextHelper
	{
		public static HttpContext GetContextWithIp(string address)
		{
			(HttpContext context, HttpConnectionFeature feature) = CreateHttpContext();
			feature.RemoteIpAddress = IPAddress.Parse(address);
			return context;
		}

		public static (HttpContext context, HttpConnectionFeature feature) CreateHttpContext()
		{
			HttpConnectionFeature feature = new();

			FeatureCollection features = new();
			features.Set<IHttpConnectionFeature>(feature);

			Mock<HttpContext> contextMock = new();
			contextMock.SetupGet(c => c.Features).Returns(features);

			return (contextMock.Object, feature);
		}
	}
}
