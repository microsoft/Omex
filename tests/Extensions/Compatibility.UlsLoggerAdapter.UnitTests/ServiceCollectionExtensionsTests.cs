// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.System.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Compatibility.UlsLoggerAdapter.UnitTests
{
	[TestClass]
	public class ServiceCollectionExtensionsTests
	{
		[TestMethod]
		public void AddUlsLoggerAddapter_SubscribesToEvents()
		{
			Mock<ILogger> mockLogger = new Mock<ILogger>();
			Mock<ILoggerFactory> mockFactory = new Mock<ILoggerFactory>();
			mockFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

			new HostBuilder()
				.ConfigureServices(collection =>
				{
					collection
						.AddSingleton(mockFactory.Object)
						.AddUlsLoggerAddapter();
				})
				.Build()
				.Start();

			uint eventId = 1;
			Category category = new Category("Test");
			Level logLevel = Levels.Error;
			string logMessage = "TestLogMessage";
			Exception exception = new Exception();

			mockLogger.Invocations.Clear();
			ULSLogging.LogTraceTag(eventId, category, logLevel, logMessage);
			Assert.AreEqual(1, mockLogger.Invocations.Count, "ULSLogging.LogTraceTag not calling ILogger");

			mockLogger.Invocations.Clear();
			ULSLogging.ReportExceptionTag(eventId, category, exception, logMessage);
			Assert.AreEqual(1, mockLogger.Invocations.Count, "ULSLogging.ReportExceptionTag not calling ILogger");
		}
	}
}
