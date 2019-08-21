// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.TimedScopes;
using Microsoft.Omex.System.TimedScopes.ReplayEventLogging;
using Microsoft.Omex.System.UnitTests.Shared;
using Microsoft.Omex.System.UnitTests.Shared.Diagnostics;
using Microsoft.Omex.System.UnitTests.Shared.TimedScopes;
using Moq;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Omex.System.UnitTests.TimedScopes
{
	/// <summary>
	/// Unit tests for verifying functionality of TimedScope class
	/// </summary>
	public sealed class TimedScopeUnitTests : UnitTestBase
	{

		[Fact]
		public void Scope_TimedScopeLogger_IsCalled()
		{
			Mock<ITimedScopeLogger> timedScopeLoggerMock = new Mock<ITimedScopeLogger>();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			TimedScope scope;
			CorrelationData data = new CorrelationData();

			using (scope = new TimedScopeDefinition("TestScope")
				.Create(data, machineInformation, customLogger: timedScopeLoggerMock.Object, replayEventConfigurator: replyEventConfiguratorMock.Object,
				timedScopeStackManager: timedScopeStackManager, initialResult: TimedScopeResult.SystemError))
			{
				timedScopeLoggerMock.Verify(x => x.LogScopeStart(scope), Times.Once);
				timedScopeLoggerMock.Verify(x => x.LogScopeEnd(scope, It.IsAny<CorrelationData>()), Times.Never);
			}

			timedScopeLoggerMock.Verify(x => x.LogScopeEnd(scope, It.IsAny<CorrelationData>()), Times.Once);
		}


		[Fact]
		public void Start_ShouldConstructTimedScope_WithTimerActive()
		{
			Mock<ITimedScopeLogger> timedScopeLoggerMock = new Mock<ITimedScopeLogger>();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			using (TimedScope scope = TestHooks.CreateTestCountersUnitTestTimedScope(false, true, machineInformation, timedScopeLoggerMock.Object,
				replyEventConfiguratorMock.Object, timedScopeStackManager))
			{
				Assert.True(scope.IsScopeActive, "Starting a scope should start the timer.");

				Assert.True(scope.IsSuccessful.HasValue, "Starting a scope should start the timer.");
				Assert.False(scope.IsSuccessful.Value, "IsSuccessful should be set to false.");
			}
		}


		[Fact]
		public void Create_ShouldConstructTimedScope_WithTimerInactive()
		{
			Mock<ITimedScopeLogger> timedScopeLoggerMock = new Mock<ITimedScopeLogger>();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			using (TimedScope scope = TestHooks.CreateTestCountersUnitTestTimedScope(true, false, machineInformation, timedScopeLoggerMock.Object,
				replyEventConfiguratorMock.Object, timedScopeStackManager))
			{
				Assert.False(scope.IsScopeActive, "Creating a scope should not start the timer.");

				Assert.True(scope.IsSuccessful.HasValue, "IsSuccessful should be set.");
				Assert.True(scope.IsSuccessful.Value, "IsSuccessful should be set to true.");
			}
		}


		[Fact]
		public void AbortTimer_ShouldDisableTimerActive()
		{
			Mock<ITimedScopeLogger> timedScopeLoggerMock = new Mock<ITimedScopeLogger>();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			using (TimedScope scope = TestHooks.CreateDefaultTimedScope(timedScopeLoggerMock.Object, replyEventConfiguratorMock.Object, machineInformation: machineInformation, 
				timedScopeStackManager: timedScopeStackManager))
			{
				Assert.True(scope.IsScopeActive, "Default scope should have timer active.");

				scope.AbortTimer();
				Assert.False(scope.IsScopeActive, "Aborting timer should stop timer.");
			}
		}


		[Fact]
		public void AbortTimer_ShouldDisableTimerActive_AndSetResultsToTrue()
		{
			Mock<ITimedScopeLogger> timedScopeLoggerMock = new Mock<ITimedScopeLogger>();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			using (TimedScope scope = TestHooks.CreateDefaultTimedScope(timedScopeLoggerMock.Object, replyEventConfiguratorMock.Object, machineInformation: machineInformation, 
				timedScopeStackManager: timedScopeStackManager))
			{
				Assert.True(scope.IsScopeActive, "Default scope should have timer active.");

				scope.AbortTimer(true);
				Assert.False(scope.IsScopeActive, "Aborting timer should stop timer.");
				Assert.True(scope.IsSuccessful.HasValue, "IsSuccessful should be set.");
				Assert.True(scope.IsSuccessful.Value, "IsSuccesful should be set to true.");
			}
		}


		[Fact]
		public void AbortTimer_ShouldDisableTimerActive_AndSetResultsToFalse()
		{
			Mock<ITimedScopeLogger> timedScopeLoggerMock = new Mock<ITimedScopeLogger>();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			using (TimedScope scope = TestHooks.CreateDefaultTimedScope(scopeLogger: timedScopeLoggerMock.Object, replayEventConfigurator: replyEventConfiguratorMock.Object,
				machineInformation: machineInformation, timedScopeStackManager: timedScopeStackManager, startScope: false))
			{
				Assert.False(scope.IsScopeActive, "Default scope started without an active scope should have timer active.");

				scope.Start();

				Assert.True(scope.IsScopeActive, "Default scope should have timer active.");

				scope.AbortTimer(false);
				Assert.False(scope.IsScopeActive, "Aborting timer should stop timer.");
				Assert.True(scope.IsSuccessful.HasValue, "IsSuccessful should be set.");
				Assert.False(scope.IsSuccessful.Value, "IsSuccesful should be set to false.");
			}
		}


		[Fact]
		public void AddLoggingValue_ShouldOutputValueInLogEvent()
		{
			Mock<ITimedScopeLogger> timedScopeLoggerMock = new Mock<ITimedScopeLogger>();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			using (TimedScope scope = TestHooks.CreateTestCountersUnitTestTimedScope(machineInformation: machineInformation, scopeLogger: timedScopeLoggerMock.Object,
				replayEventConfigurator: replyEventConfiguratorMock.Object, timedScopeStackManager: timedScopeStackManager))
			{
				scope.AddLoggingValue(TimedScopeDataKeys.Category, "MyCategory");
				scope.End();

				// There should be one 'Ending' transaction log call with formatted output
				foreach (LogEventArgs args in LoggedEvents)
				{
					if (args.CategoryId == Categories.TimingGeneral)
					{
						if (args.FullMessage.Contains("Ending timed scope"))
						{
							Assert.Contains("Category:'MyCategory';", args.FullMessage, StringComparison.Ordinal);
						}
					}
				}
			}
		}


		[Fact]
		public void FailedScope_ResultAndFailureDescription_ShouldOutputValueInLogEvent()
		{
			FailOnErrors = false;

			UnitTestTimedScopeLogger unitTestTimedScopeLogger = new UnitTestTimedScopeLogger();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			using (TimedScope scope = TestHooks.CreateTestCountersUnitTestTimedScope(machineInformation: machineInformation, scopeLogger: unitTestTimedScopeLogger,
				replayEventConfigurator: replyEventConfiguratorMock.Object, timedScopeStackManager: timedScopeStackManager))
			{
				scope.Result = TimedScopeResult.ExpectedError;
				scope.FailureDescription = UnitTestFailureDescription.ExampleDescription;
			}

			TimedScopeLogEvent scopeEvent = unitTestTimedScopeLogger.Events.SingleOrDefault();

			if (VerifyNotNullAndReturn(scopeEvent, "Scope end event should be logged"))
			{
				Assert.Equal(scopeEvent.Result, TimedScopeResult.ExpectedError);
				Assert.Equal(scopeEvent.FailureDescription, UnitTestFailureDescription.ExampleDescription.ToString());
			}
		}


		[Fact]
		public void SucceededScope_Result_ShouldOutputValueInLogEvent()
		{

			UnitTestTimedScopeLogger unitTestTimedScopeLogger = new UnitTestTimedScopeLogger();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			using (TimedScope scope = TestHooks.CreateTestCountersUnitTestTimedScope(machineInformation: machineInformation, scopeLogger: unitTestTimedScopeLogger,
				replayEventConfigurator: replyEventConfiguratorMock.Object, timedScopeStackManager: timedScopeStackManager))
			{
				scope.Result = TimedScopeResult.Success;
			}

			TimedScopeLogEvent scopeEvent = unitTestTimedScopeLogger.Events.SingleOrDefault();

			if (VerifyNotNullAndReturn(scopeEvent, "Timed scope should be logged"))
			{
				Assert.Equal(scopeEvent.Result, TimedScopeResult.Success);
			}
		}


		[Fact]
		public void AddLoggingValue_WithNullKey_ShouldLogError()
		{
			FailOnErrors = false;

			Mock<ITimedScopeLogger> timedScopeLoggerMock = new Mock<ITimedScopeLogger>();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			using (TimedScope scope = TestHooks.CreateDefaultTimedScope(timedScopeLoggerMock.Object, replyEventConfiguratorMock.Object, machineInformation, timedScopeStackManager))
			{
				scope.AddLoggingValue(null, "My Application.");

				Assert.Equal(TraceErrors.Count(), 1);
				LoggedEvents.Clear();
			}
		}


		[Fact]
		public void Start_DisposedTimedScope_ShoudLogError()
		{
			FailOnErrors = false;

			Mock<ITimedScopeLogger> timedScopeLoggerMock = new Mock<ITimedScopeLogger>();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			TimedScope scope = TestHooks.CreateDefaultTimedScope(timedScopeLoggerMock.Object, replyEventConfiguratorMock.Object, machineInformation, timedScopeStackManager);
			scope.Dispose();

			scope.Start();

			Assert.Equal(TraceErrors.Count(), 1);
			LoggedEvents.Clear();
		}


		[Fact]
		public void Start_WithActiveTimer_ShouldLogError()
		{
			FailOnErrors = false;

			Mock<ITimedScopeLogger> timedScopeLoggerMock = new Mock<ITimedScopeLogger>();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			using (TimedScope scope = TestHooks.CreateDefaultTimedScope(timedScopeLoggerMock.Object, replyEventConfiguratorMock.Object, machineInformation, timedScopeStackManager))
			{
				Assert.True(scope.IsScopeActive, "Timer should be active.");

				scope.Start();

				Assert.Equal(TraceErrors.Count(), 1);
				LoggedEvents.Clear();
			}
		}


		[Fact]
		public void End_WithDisposedTimedScope_ShouldLogError()
		{
			FailOnErrors = false;

			Mock<ITimedScopeLogger> timedScopeLoggerMock = new Mock<ITimedScopeLogger>();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			using (TimedScope scope = TestHooks.CreateDefaultTimedScope(timedScopeLoggerMock.Object, replyEventConfiguratorMock.Object, machineInformation, timedScopeStackManager))
			{
				Assert.True(scope.IsScopeActive, "Timer should be active.");

				scope.Dispose();

				Assert.False(scope.IsScopeActive, "Dispose should turn off timer.");

				scope.End();

				Assert.Equal(TraceErrors.Count(), 1);
				LoggedEvents.Clear();
			}
		}


		[Fact]
		public void Frequency_ShouldMatchFrequencyOfStopwatch()
		{
			Assert.Equal(TimedScope.Frequency, Stopwatch.Frequency);
		}


		[Fact]
		public void NotSettingTimedScopeResult_ChangesToSystemError()
		{
			LoggedEvents.Clear();

			UnitTestTimedScopeLogger unitTestTimedScopeLogger = new UnitTestTimedScopeLogger();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			using (TimedScope scope = TestHooks.CreateDefaultTimedScope(unitTestTimedScopeLogger, replyEventConfiguratorMock.Object, machineInformation, timedScopeStackManager))
			{
			}

			TimedScopeLogEvent evt = unitTestTimedScopeLogger.SingleTimedScopeEvent(TestHooks.DefaultTimedScopeName);
			if (VerifyNotNullAndReturn(evt, "A scope event has been logged"))
			{
				Assert.Equal(evt.Result, TimedScopeResult.SystemError);
			}
		}


		[Fact]
		public void DefaultTimedScopeResult_IsSeparateValue()
		{
			// Default value should never be used explicitly and is marked as Obsolete for that reason, so it can't be used.
			// But we need a separate default value to support the "not yet set" state. Verify the default is actually separate
			// and does not map to any of the real result values.
			TimedScopeResult result = default(TimedScopeResult);

			Assert.NotEqual(TimedScopeResult.Success, result);
			Assert.NotEqual(TimedScopeResult.SystemError, result);
			Assert.NotEqual(TimedScopeResult.ExpectedError, result);
		}


		[Fact]
		public void DefaultTimedScopeResult_LogsAsSystemError()
		{
			LoggedEvents.Clear();

			CorrelationData data = new CorrelationData();

			UnitTestTimedScopeLogger unitTestTimedScopeLogger = new UnitTestTimedScopeLogger();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			IMachineInformation machineInformation = new UnitTestMachineInformation();
			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManagerMock.Object, machineInformation);

			using (TimedScope.Create(data, machineInformation, TestHooks.DefaultTimedScopeName, "description", unitTestTimedScopeLogger, 
				replyEventConfiguratorMock.Object, timedScopeStackManager, default(TimedScopeResult)))
			{
			}

			TimedScopeLogEvent evt = unitTestTimedScopeLogger.SingleTimedScopeEvent(TestHooks.DefaultTimedScopeName);
			if (VerifyNotNullAndReturn(evt, "A scope event has been logged"))
			{
				Assert.Equal(TimedScopeResult.SystemError, evt.Result);
			}
		}


		/// <summary>
		/// Unit test failure descriptions
		/// </summary>
		private enum UnitTestFailureDescription
		{
			/// <summary>
			/// Example description
			/// </summary>
			ExampleDescription,
		}


		/// <summary>
		/// Verifies that a reference type is not null and returns the evaluation result.
		/// </summary>
		/// <typeparam name="T">The reference type.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="message">The logged message.</param>
		/// <param name="args">Format string args.</param>
		/// <returns>The evaluation result.</returns>
		private bool VerifyNotNullAndReturn<T>(T value, string message, params object[] args) where T : class
		{
			try
			{
				Assert.NotNull(value);
				return value != null;
			}
			catch (NotNullException)
			{
				Console.WriteLine(message, args);
				throw;
			}
		}


		/// <summary>
		/// Trace errors
		/// </summary>
		private IEnumerable<LogEventArgs> TraceErrors
		{
			get
			{
				return LoggedEvents.Except(ReportExceptions).Where((args) => { return args.Level == Levels.Error; });
			}
		}


		/// <summary>
		/// Exceptions reported
		/// </summary>
		private IEnumerable<ReportExceptionEventArgs> ReportExceptions
		{
			get
			{
				IEnumerable<LogEventArgs> events = LoggedEvents;

				return LoggedEvents.OfType<ReportExceptionEventArgs>();
			}
		}
	}
}
