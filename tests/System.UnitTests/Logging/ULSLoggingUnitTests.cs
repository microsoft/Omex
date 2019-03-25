// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.UnitTests.Shared;
using Xunit;
using UntaggedLogging = Microsoft.Omex.System.Logging.ULSLogging;

namespace Microsoft.Omex.System.UnitTests.Logging
{
	/// <summary>
	/// Unit tests for ULSLogging class
	/// </summary>
	public sealed class ULSLoggingUnitTests : UnitTestBase
	{
		[Theory]
		[InlineData(Levels.LogLevel.Error)]
		[InlineData(Levels.LogLevel.Warning)]
		[InlineData(Levels.LogLevel.Info)]
		[InlineData(Levels.LogLevel.Verbose)]
		[InlineData(Levels.LogLevel.Spam)]
		public void LogTraceTag_ShouldLog(Levels.LogLevel logLevel)
		{
			FailOnErrors = false;

			UntaggedLogging.LogTraceTag(0, Categories.ArgumentValidation, Levels.LevelFromLogLevel(logLevel), "Test message");

			Assert.Equal(1, LoggedEvents.Count);
			Assert.Equal(Levels.LevelFromLogLevel(logLevel), LoggedEvents[0].Level);
		}


		[Fact]
		public void ReportExceptionTag_ShouldLog()
		{
			FailOnErrors = false;

			string message = "Should log exception";
			UntaggedLogging.ReportExceptionTag(0, Categories.ArgumentValidation, new ArgumentNullException(), message);

			Assert.Equal(1, LoggedEvents.Count);
			ReportExceptionEventArgs exceptionEventArgs = LoggedEvents[0] as ReportExceptionEventArgs;
			Assert.NotNull(exceptionEventArgs);
			Assert.Equal(message, exceptionEventArgs.Message);
			Assert.Equal(Levels.Error, exceptionEventArgs.Level);
			Assert.NotNull(exceptionEventArgs.Exception);
			Assert.Equal(typeof(ArgumentNullException), exceptionEventArgs.Exception.GetType());
			Assert.Equal(Categories.ArgumentValidation, exceptionEventArgs.Category);
		}


		[Fact]
		public void RaiseLogEvent_WithoutEvent_DoesNotLog()
		{
			UntaggedLogging.RaiseLogEvent(null, null);
			Assert.Empty(LoggedEvents);
		}


		[Fact]
		public void RaiseLogEvent_WithEvent_Logs()
		{
			LogEventArgs eventArgs = new LogEventArgs(0, Categories.ArgumentValidation, Levels.Warning, "Message");
			UntaggedLogging.RaiseLogEvent(null, eventArgs);
			Assert.Equal(1, LoggedEvents.Count);
			Assert.Same(eventArgs, LoggedEvents[0]);
		}
	}
}