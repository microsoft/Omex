using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Compatability.Logger;
using Microsoft.Omex.Extensions.Compatability.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Compatability.UnitTests
{
	[TestClass]
	public class ServiceCollectionExtensionsTests
	{
		[TestMethod]
		public void CheckThatTypesRegistred()
		{
			Mock<ILogger> mockLogger = new Mock<ILogger>();
			Mock<ILoggerFactory> mockFactory = new Mock<ILoggerFactory>();
			mockFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

			new HostBuilder()
				.ConfigureServices((context, collection) => collection.AddSingleton(mockFactory.Object))
				.AddOmexCompatabilityServices()
				.Build()
				.Start();

			EventId eventId = new EventId(1);
			Category category = new Category("Test");
			LogLevel logLevel = LogLevel.Error;
			string logMessage = "TestLogMessage";
			Exception exception = new Exception();

			mockLogger.Invocations.Clear();
			ULSLogging.LogTraceTag(eventId, category, logLevel, logMessage);
			Assert.AreEqual(1, mockLogger.Invocations.Count, "ULSLogging.LogTraceTag not calling ILogger");

			mockLogger.Invocations.Clear();
			ULSLogging.ReportExceptionTag(eventId, category, exception, logMessage);
			Assert.AreEqual(1, mockLogger.Invocations.Count, "ULSLogging.ReportExceptionTag not calling ILogger");

			mockLogger.Invocations.Clear();
			Code.Validate(false, logMessage, eventId);
			Assert.AreEqual(1, mockLogger.Invocations.Count, "Code.Validate not calling ILogger");
		}
	}
}
