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

			EventId logEventId = new EventId(1);
			Category category = new Category("Test");
			const LogLevel logLevel = LogLevel.Error;
			const string logMessage = "TestLogMessage";

			mockLogger.Invocations.Clear();
			ULSLogging.LogTraceTag(logEventId, category, logLevel, logMessage);
			Assert.AreEqual(1, mockLogger.Invocations.Count, "ULSLogging not calling ILogger");

			EventId validationEventId = new EventId(2);
			string validationMessage = "TestValidationMessage";

			mockLogger.Invocations.Clear();
			Code.Validate(false, validationMessage, validationEventId);
			Assert.AreEqual(1, mockLogger.Invocations.Count, "Code.Validate not calling ILogger");
		}
	}
}
