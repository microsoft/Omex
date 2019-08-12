// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.Omex.System.Context;
using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.TimedScopes;
using Microsoft.Omex.System.TimedScopes.ReplayEventLogging;
using Microsoft.Omex.System.UnitTests.Shared.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Assert = Xunit.Assert;

namespace Microsoft.Omex.System.AspNetCore.UnitTests
{
	/// <summary>
	/// Unit tests for verifying functionality of ReplayEventConfigurator class
	/// </summary>
	[TestClass]
	public class ReplayEventConfiguratorUnitTests : AspNetCoreUnitTestsBase
	{

		[TestInitialize]
		public void TestInitialize()
		{
			SetHttpContextCurrent();

			CallContextManagerInstance = CreateCallContextManager();
			CallContextManagerInstance.CallContextOverride = new HttpCallContext(useLogicalCallContext: true);

			MachineInformation = new UnitTestMachineInformation();

			HttpCallContext callContext = CallContextManagerInstance.CallContextHandler(MachineInformation) as HttpCallContext;
			callContext.ExistingCallContext();
			callContext.StartCallContext();
		}


		[TestCleanup]
		public void end()
		{
			ICallContext context = CallContextManagerInstance.CallContextOverride;

			ICallContext existingContext = context.ExistingCallContext();
			Correlation.CorrelationEnd();
			context.EndCallContext();
		}


		[TestMethod]
		public void SuccessTimedScope_DoesntReplayLogs()
		{
			Mock<ITimedScopeLogger> timedScopeLoggerMock = new Mock<ITimedScopeLogger>();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();

			Correlation = new Correlation(new MemoryCorrelationHandler(), CallContextManagerInstance, MachineInformation);
			Correlation.CorrelationStart(new CorrelationData());

			CorrelationData currentCorrelation = Correlation.CurrentCorrelation;

			Assert.False(currentCorrelation.ShouldReplayUls);

			using (TimedScope scope = TimedScope.Start(currentCorrelation, MachineInformation, "TestScope",
				customLogger: timedScopeLoggerMock.Object, replayEventConfigurator: replyEventConfiguratorMock.Object))
			{
				scope.Result = TimedScopeResult.Success;

				Mock<IReplayEventDisabledTimedScopes> disabledScopes = new Mock<IReplayEventDisabledTimedScopes>();
				disabledScopes.Setup(x => x.IsDisabled(scope.ScopeDefinition)).Returns(false);

				ReplayEventConfigurator configurator = new ReplayEventConfigurator(disabledScopes.Object, Correlation);
				configurator.ConfigureReplayEventsOnScopeEnd(scope);
			}

			Assert.False(currentCorrelation.ShouldReplayUls);
		}


		[TestMethod]
		public void FailedTimedScope_ShouldReplayLogs()
		{

			Mock<ITimedScopeLogger> timedScopeLoggerMock = new Mock<ITimedScopeLogger>();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();
			Mock<ILogEventCache> mockCache = new Mock<ILogEventCache>();

			Correlation = new Correlation(new MemoryCorrelationHandler(), CallContextManagerInstance, MachineInformation);
			Correlation.CorrelationStart(new CorrelationData(mockCache.Object));

			CorrelationData currentCorrelation = Correlation.CurrentCorrelation;

			Assert.False(currentCorrelation.ShouldReplayUls, "Logs shouldn't be replayed");

			using (TimedScope scope = TimedScope.Start(currentCorrelation, MachineInformation, "TestScope",
				customLogger: timedScopeLoggerMock.Object, replayEventConfigurator: replyEventConfiguratorMock.Object))
			{
				scope.Result = TimedScopeResult.SystemError;

				Mock<IReplayEventDisabledTimedScopes> disabledScopes = new Mock<IReplayEventDisabledTimedScopes>();
				disabledScopes.Setup(x => x.IsDisabled(scope.ScopeDefinition)).Returns(false);

				ReplayEventConfigurator configurator = new ReplayEventConfigurator(disabledScopes.Object, Correlation);
				configurator.ConfigureReplayEventsOnScopeEnd(scope);
			}

			Assert.True(currentCorrelation.ShouldReplayUls, "Logs should be replayed");
		}


		[TestMethod]
		public void FailedDisabledTimedScope_DoesntReplayLogs()
		{
			Mock<ITimedScopeLogger> timedScopeLoggerMock = new Mock<ITimedScopeLogger>();
			Mock<IReplayEventConfigurator> replyEventConfiguratorMock = new Mock<IReplayEventConfigurator>();

			Correlation = new Correlation(new MemoryCorrelationHandler(), CallContextManagerInstance, MachineInformation);
			Correlation.CorrelationStart(new CorrelationData());

			CorrelationData currentCorrelation = Correlation.CurrentCorrelation;

			Assert.False(currentCorrelation.ShouldReplayUls, "Logs shouldn't be replayed");

			using (TimedScope scope = TimedScope.Start(currentCorrelation, MachineInformation, "TestScope",
				customLogger: timedScopeLoggerMock.Object, replayEventConfigurator: replyEventConfiguratorMock.Object))
			{
				scope.Result = TimedScopeResult.SystemError;

				Mock<IReplayEventDisabledTimedScopes> disabledScopes = new Mock<IReplayEventDisabledTimedScopes>();
				disabledScopes.Setup(x => x.IsDisabled(scope.ScopeDefinition)).Returns(true);

				ReplayEventConfigurator configurator = new ReplayEventConfigurator(disabledScopes.Object, Correlation);
				configurator.ConfigureReplayEventsOnScopeEnd(scope);
			}

			Assert.False(currentCorrelation.ShouldReplayUls, "Logs shouldn't be replayed");
		}


		private ICallContextManager CreateCallContextManager()
		{
			return new CallContextManager();
		}


		private Correlation Correlation { get; set; }


		private ICallContextManager CallContextManagerInstance { get; set; }


		private IMachineInformation MachineInformation { get; set; }


		/// <summary>
		/// Create an HttpContext and configure HttpContext.Current
		/// </summary>
		private static void SetHttpContextCurrent()
		{
			HttpContext httpContext = new DefaultHttpContext();
			httpContext.Request.Host = new HostString("localhost");

			m_httpContextAccessor.Value.HttpContext = httpContext;
		}
	}
}
