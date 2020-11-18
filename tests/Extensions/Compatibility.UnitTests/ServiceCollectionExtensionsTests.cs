// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.Compatibility.Logger;
using Microsoft.Omex.Extensions.Compatibility.TimedScopes;
using Microsoft.Omex.Extensions.Compatibility.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Compatibility.UnitTests
{
	[TestClass]
	public class ServiceCollectionExtensionsTests
	{
		[TestMethod]
		public void AddOmexCompatibilityServices_RegisterTypes()
		{
			Mock<ILogger> mockLogger = new Mock<ILogger>();
			Mock<ILoggerFactory> mockFactory = new Mock<ILoggerFactory>();
			mockFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

			new HostBuilder()
				.ConfigureServices(collection =>
				{
					collection
						.AddSingleton(mockFactory.Object)
						.AddOmexCompatibilityServices()
						.AddOmexActivitySource();
				})
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

			using (Activity? startedTimedScope = TimedScope.CreateAndStart("TestStartedTimedScope", ActivityResult.SystemError))
			{
				AssertResult(ActivityResultStrings.SystemError);
			}
		}

		private static void AssertResult(string expectedResult)
		{
			string? value = Activity.Current?.Tags.FirstOrDefault(p => string.Equals(p.Key, ActivityTagKeys.Result, StringComparison.Ordinal)).Value;
			Assert.AreEqual(expectedResult, value);
		}
	}
}
