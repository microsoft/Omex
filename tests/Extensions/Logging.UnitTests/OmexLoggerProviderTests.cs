using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	public class OmexLoggerProviderTests
	{
		[TestMethod]
		public void OmexLoggerProvider()
		{
			string testCategory = "SomeCategoryName";
			ILogsEventSource mockEventSource = new Mock<ILogsEventSource>().Object;
			IExternalScopeProvider mockExternalScopeProvider = new Mock<IExternalScopeProvider>().Object;

			ILoggerProvider loggerProvider = new OmexLoggerProvider(mockEventSource, mockExternalScopeProvider);
			ILogger logger = loggerProvider.CreateLogger(testCategory);
		}
	}
}
