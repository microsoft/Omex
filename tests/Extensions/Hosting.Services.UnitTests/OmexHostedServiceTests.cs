﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	[TestClass]
	public class OmexHostedServiceTests
	{
		[TestMethod]
		public async Task StartAsync_ProperlyCanceled()
		{
			MockRunner runnerMock = new MockRunner(t => Task.Delay(int.MaxValue, t));
			ILogger<OmexHostedService> loggerMock = new NullLogger<OmexHostedService>();
			IHostLifetime lifetimeMock = new Mock<IHostLifetime>().Object;

			OmexHostedService hostedService = new OmexHostedService(runnerMock,lifetimeMock, loggerMock);

			Assert.IsFalse(runnerMock.IsStarted, "RunServiceAsync should not be called after constructor");

			await hostedService.StartAsync(CancellationToken.None).ConfigureAwait(false);
			Assert.IsTrue(runnerMock.IsStarted, "RunServiceAsync should be called after StartAsync");

			Task task = runnerMock.Task!;
			Assert.IsFalse(task.IsCanceled, "Task should not be canceled");
			Assert.IsFalse(runnerMock.Token.IsCancellationRequested, "CancelationToken should not be canceled");

			await hostedService.StopAsync(CancellationToken.None).ConfigureAwait(false);
			Assert.IsTrue(runnerMock.Token.IsCancellationRequested, "Task should be canceled");
			Assert.IsTrue(task.IsCanceled, "CancelationToken should be canceled");
		}

		[TestMethod]
		public async Task StartAsync_HandlesExceptions()
		{
			MockRunner runnerMock = new MockRunner(t => Task.Run(async () =>
				{
					await Task.Delay(5).ConfigureAwait(false);
					throw new ArithmeticException("Totaly valid exeption");
				}));
			ILogger<OmexHostedService> loggerMock = new NullLogger<OmexHostedService>();
			IHostLifetime lifetimeMock = new Mock<IHostLifetime>().Object;

			OmexHostedService hostedService = new OmexHostedService(runnerMock, lifetimeMock, loggerMock);
			Assert.IsFalse(runnerMock.IsStarted, "RunServiceAsync should not be called after constructor");

			await hostedService.StartAsync(CancellationToken.None).ConfigureAwait(false);
			Assert.IsTrue(runnerMock.IsStarted, "RunServiceAsync should be called after StartAsync");
			Assert.IsFalse(runnerMock.Token.IsCancellationRequested, "CancelationToken should not be canceled");

			await hostedService.StopAsync(CancellationToken.None).ConfigureAwait(false);
			Assert.IsTrue(runnerMock.Task!.IsFaulted, "Task should be faulted");
			Assert.IsTrue(runnerMock.Token.IsCancellationRequested, "Task should be canceled");
		}

		[TestMethod]
		public async Task TaskCompletion_StopsHost()
		{
			TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
			MockRunner runnerMock = new MockRunner(t => completionSource.Task);
			ILogger<OmexHostedService> loggerMock = new NullLogger<OmexHostedService>();
			Mock<IHostLifetime> lifetimeMock = new Mock<IHostLifetime>();

			OmexHostedService hostedService = new OmexHostedService(runnerMock, lifetimeMock.Object, loggerMock);

			Assert.IsFalse(runnerMock.IsStarted, "RunServiceAsync should not be called after constructor");

			await hostedService.StartAsync(CancellationToken.None).ConfigureAwait(false);
			Assert.IsTrue(runnerMock.IsStarted, "RunServiceAsync should be called after StartAsync");

			completionSource.SetResult(true);
			lifetimeMock.Verify(l => l.StopAsync(It.IsAny<CancellationToken>()), Times.Once(), "Should stop host after task completion");
		}

		private class MockRunner : IOmexServiceRunner
		{
			public CancellationToken Token { get; private set; }

			public Task? Task { get; private set; }

			public bool IsStarted { get; private set; }

			public Task RunServiceAsync(CancellationToken cancellationToken)
			{
				IsStarted = true;
				Token = cancellationToken;
				return Task = m_function(cancellationToken);
			}

			public MockRunner(Func<CancellationToken, Task> function) => m_function = function;

			private readonly Func<CancellationToken, Task> m_function;
		}
	}
}
