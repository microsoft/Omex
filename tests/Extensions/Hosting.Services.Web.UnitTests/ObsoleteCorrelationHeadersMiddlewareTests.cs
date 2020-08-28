using System;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests
{
	[TestClass]
	public sealed class ObsoleteCorrelationHeadersMiddlewareTests
	{
		[Obsolete]
		[TestInitialize]
		public void Initialize()
		{
			TestActivity = new Activity("ObsoleteCorrelationHeadersMiddlewareTests");
			TestActivity.Start();
			TestMiddleware = new ObsoleteCorrelationHeadersMiddleware();
			TestHttpContext = new DefaultHttpContext();
		}

		[TestCleanup]
		public void Cleanup()
		{
			TestActivity.Stop();
		}

		[Obsolete]
		[DataTestMethod]
		[DataRow("MS-CorrelationId")]
		[DataRow("ms-correlationid")]
		[DataRow("corr")]
		[DataRow("HTTP_X_CORRELATIONID")]
		public async Task SetsObsoleteCorrelationIdFromHeaderToActivity(string correlationName)
		{
			// Arrange
			Guid obsoleteCorrelation = Guid.NewGuid();
			TestHttpContext.Request.Headers.Add(correlationName, obsoleteCorrelation.ToString());

			// Act
			await TestMiddleware.InvokeAsync(TestHttpContext, DummyDelegate);

			// Assert
			Assert.AreEqual(obsoleteCorrelation.ToString(), TestActivity.GetBaggageItem("ObsoleteCorrelationId"));
		}

		[Obsolete]
		[DataTestMethod]
		[DataRow("MS-CorrelationId")]
		[DataRow("ms-correlationid")]
		[DataRow("corr")]
		[DataRow("HTTP_X_CORRELATIONID")]
		public async Task SetsObsoleteCorrelationIdFromHeaderToHeader(string correlationName)
		{
			// Arrange
			Guid obsoleteCorrelation = Guid.NewGuid();

			// Mock the HttpResponse feature to test response header rewrite
			Mock<IHttpResponseFeature> feature = new Mock<IHttpResponseFeature>();
			HeaderDictionary dict = new HeaderDictionary();
			feature.Setup(x => x.Headers).Returns(dict);
			feature
				.Setup(x => x.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<HttpResponse>()))
				.Callback<Func<object, Task>, object>((callback, _) => callback(TestHttpContext.Response));
			TestHttpContext.Features.Set(feature.Object);

			TestHttpContext.Request.Headers.Add(correlationName, obsoleteCorrelation.ToString());

			// Act
			await TestMiddleware.InvokeAsync(TestHttpContext, DummyDelegate);

			// Assert
			TestHttpContext.Response.Headers.TryGetValue("X-CorrelationId", out StringValues value);
			Assert.AreEqual(obsoleteCorrelation.ToString(), value.ToString());
		}

		private Activity TestActivity { get; set; } = new Activity("ObsoleteCorrelationHeadersMiddlewareTests");

		[Obsolete]
		private IMiddleware TestMiddleware { get; set; } = new ObsoleteCorrelationHeadersMiddleware();

		private RequestDelegate DummyDelegate { get; set; } = (HttpContext hc) => Task.CompletedTask;

		private HttpContext TestHttpContext { get; set; } = new DefaultHttpContext();
	}
}
