// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	public class OmexLoggerProviderTests
	{
		[TestMethod]
		public void CreateLogger_PropagatesCategory()
		{
			string testCategory = "SomeCategoryName";
			string testMessage = "TestMessage";
			Mock<ILogEventSender> mockEventSource = new Mock<ILogEventSender>();
			IExternalScopeProvider mockExternalScopeProvider = new Mock<IExternalScopeProvider>().Object;

			ILoggerProvider loggerProvider = new OmexLoggerProvider(mockEventSource.Object, mockExternalScopeProvider);
			ILogger logger = loggerProvider.CreateLogger(testCategory);

			Assert.IsInstanceOfType(logger, typeof(OmexLogger));

			mockEventSource.Setup(e => e.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

			logger.LogError(testMessage);

			mockEventSource.Verify(e => e.LogMessage(It.IsAny<Activity>(), testCategory, LogLevel.Error, It.IsAny<EventId>(), It.IsAny<int>(), testMessage, It.IsAny<Exception>()), Times.Once);
		}
	}
}
