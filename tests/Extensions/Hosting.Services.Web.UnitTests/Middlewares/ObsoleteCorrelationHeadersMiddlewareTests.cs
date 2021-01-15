using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

		private Activity TestActivity { get; set; } = new Activity("ObsoleteCorrelationHeadersMiddlewareTests");

		[Obsolete]
		private IMiddleware TestMiddleware { get; set; } = new ObsoleteCorrelationHeadersMiddleware();

		private RequestDelegate DummyDelegate { get; set; } = (HttpContext hc) => Task.CompletedTask;

		private HttpContext TestHttpContext { get; set; } = new DefaultHttpContext();
	}
}
