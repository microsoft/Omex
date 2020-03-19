// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using Microsoft.Omex.System.Context;
using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.TimedScopes;
using Microsoft.Omex.System.UnitTests.Shared;
using Microsoft.Omex.System.UnitTests.Shared.Diagnostics;
using Moq;
using Xunit;

namespace Microsoft.Omex.System.UnitTests.TimedScopes
{
	/// <summary>
	/// Unit tests for verifying functionality of TimedScope class
	/// </summary>
	public sealed class TimedScopeStackManagerUnitTests : UnitTestBase
	{

		[Fact]
		public void GetTimedScopeStack_MachineInformation_Null_Value()
		{
			FailOnErrors = false;

			Mock<ICallContextManager> callContextManagerMock = new Mock<ICallContextManager>();

			Assert.Throws<ArgumentNullException>(() =>
			{
				new TimedScopeStackManager(callContextManagerMock.Object, null);
			});
		}

		[Fact]
		public void GetTimedScopeStack_CallContextManager_Null_Value()
		{
			FailOnErrors = false;

			IMachineInformation machineInformation = new UnitTestMachineInformation();

			Assert.Throws<ArgumentNullException>(() =>
			{
				new TimedScopeStackManager(null, machineInformation);
			});
		}

		[Fact]
		public void GetTimedScopeStack_ShouldReturnValue()
		{
			IMachineInformation machineInformation = new UnitTestMachineInformation();
			Mock<ICallContextManager> callContextManager = new Mock<ICallContextManager>();
			Mock<ICallContext> callContext = new Mock<ICallContext>();

			callContext.Setup(mock => mock.Data).Returns(new ConcurrentDictionary<string, object>(StringComparer.Ordinal));
			callContextManager.SetupGet(mock => mock.CallContextOverride).Returns(callContext.Object);
			callContextManager.Setup(mock => mock.CallContextHandler(It.IsAny<IMachineInformation>())).Returns(callContext.Object);

			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManager.Object, machineInformation);

			Assert.NotNull(timedScopeStackManager.Scopes);

			callContextManager.Verify(x => x.CallContextHandler(machineInformation), Times.Once);
			callContext.Verify(x => x.Data, Times.AtLeastOnce);
		}

		[Fact]
		public void GetTimedScopeStack_ShouldReturnNullValue()
		{
			IMachineInformation machineInformation = new UnitTestMachineInformation();
			Mock<ICallContextManager> callContextManager = new Mock<ICallContextManager>();
			ICallContext callContext = null;

			callContextManager.SetupGet(mock => mock.CallContextOverride).Returns(callContext);
			callContextManager.Setup(mock => mock.CallContextHandler(It.IsAny<IMachineInformation>())).Returns(callContext);

			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManager.Object, machineInformation);

			Assert.Null(timedScopeStackManager.Scopes);

			callContextManager.Verify(x => x.CallContextHandler(machineInformation), Times.Once);
		}

		[Fact]
		public void SetTimedScopeStack_ShouldReturnValue()
		{
			IMachineInformation machineInformation = new UnitTestMachineInformation();
			Mock<ICallContextManager> callContextManager = new Mock<ICallContextManager>();
			Mock<ICallContext> callContext = new Mock<ICallContext>();

			callContext.Setup(mock => mock.Data).Returns(new ConcurrentDictionary<string, object>(StringComparer.Ordinal));
			callContextManager.SetupGet(mock => mock.CallContextOverride).Returns(callContext.Object);
			callContextManager.Setup(mock => mock.CallContextHandler(It.IsAny<IMachineInformation>())).Returns(callContext.Object);

			ITimedScopeStackManager timedScopeStackManager = new TimedScopeStackManager(callContextManager.Object, machineInformation);

			timedScopeStackManager.Scopes = TimedScopeStack.Root;

			callContextManager.Verify(x => x.CallContextHandler(machineInformation), Times.Once);

			Assert.NotNull(timedScopeStackManager.Scopes);

		}
	}
}
