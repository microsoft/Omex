// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Logging.Scrubbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	[Obsolete("OmexLogger is Obsolete and pending for removal on 1 July 2024.", DiagnosticId = "OMEX188")]
	public class OmexLoggerProviderTests
	{
		[TestMethod]
		[DataTestMethod]
		[DataRow(true)]
		[DataRow(false)]
		public void CreateLogger_PropagatesCategory(bool omexLoggerEnabled)
		{
			const string testCategory = "SomeCategoryName";
			const string testMessage = "TestMessage";
			Mock<ILogEventSender> mockEventSource = new();
			IExternalScopeProvider mockExternalScopeProvider = new Mock<IExternalScopeProvider>().Object;

			Mock<IOptionsMonitor<OmexLoggingOptions>> mockOmexLoggingOption = new();
			OmexLoggingOptions omexLoggingOptions = new OmexLoggingOptions();
			omexLoggingOptions.OmexLoggerEnabled = omexLoggerEnabled;
			mockOmexLoggingOption.Setup(m => m.CurrentValue).Returns(omexLoggingOptions);

			ILoggerProvider loggerProvider = new OmexLoggerProvider(mockEventSource.Object, mockExternalScopeProvider, Array.Empty<ILogScrubbingRule>(), mockOmexLoggingOption.Object);
			ILogger logger = loggerProvider.CreateLogger(testCategory);

			Assert.IsInstanceOfType(logger, typeof(OmexLogger));

			mockEventSource.Setup(e => e.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

			logger.LogError(testMessage);

			mockEventSource.Verify(e => e.LogMessage(It.IsAny<Activity>(), testCategory, LogLevel.Error, It.IsAny<EventId>(), It.IsAny<int>(), testMessage, It.IsAny<Exception>()), omexLoggerEnabled ? Times.Once : Times.Never);
		}
	}
}
