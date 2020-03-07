// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hosting.Services.UnitTests
{
	[TestClass]
	public class OmexHostedServiceTests
	{
		[TestMethod]
		public async Task CheckHostStartAndCancelation()
		{
			MockRunner runnerMock = new MockRunner(t => Task.Delay(int.MaxValue, t));
			MockLifetime lifetimeMock = new MockLifetime();
			ILogger<OmexHostedService> loggerMock = new NullLogger<OmexHostedService>();

			OmexHostedService hostedService = new OmexHostedService(runnerMock, lifetimeMock, loggerMock);

			Assert.IsFalse(runnerMock.IsStarted, "RunServiceAsync should not be called after constructor");

			await hostedService.StartAsync(CancellationToken.None);

			Assert.IsFalse(runnerMock.IsStarted, "RunServiceAsync should not be called after StartAsync");

			lifetimeMock.ApplicationStartedSource.Cancel();

			Assert.IsTrue(runnerMock.IsStarted, "RunServiceAsync should not be called after StartAsync");

			Task task = runnerMock.Task!;

			Assert.IsFalse(task.IsCanceled, "Task should not be canceled");
			Assert.IsFalse(runnerMock.Token.IsCancellationRequested, "CancelationToken should not be canceled");

			lifetimeMock.ApplicationStoppingSource.Cancel();

			Assert.IsTrue(runnerMock.Token.IsCancellationRequested, "Task should be canceled");
			Assert.IsTrue(task.IsCanceled, "CancelationToken should be canceled");
		}



		[TestMethod]
		public async Task CheckHostStartAndFailure()
		{
			MockRunner runnerMock = new MockRunner(t => Task.Run(() => throw new Exception("Totaly valid exeption")));
			MockLifetime lifetimeMock = new MockLifetime();
			ILogger<OmexHostedService> loggerMock = new NullLogger<OmexHostedService>();

			OmexHostedService hostedService = new OmexHostedService(runnerMock, lifetimeMock, loggerMock);

			Assert.IsFalse(runnerMock.IsStarted, "RunServiceAsync should not be called after constructor");

			await hostedService.StartAsync(CancellationToken.None);

			Assert.IsFalse(runnerMock.IsStarted, "RunServiceAsync should not be called after StartAsync");

			lifetimeMock.ApplicationStartedSource.Cancel();

			Assert.IsTrue(runnerMock.IsStarted, "RunServiceAsync should be called after StartAsync");

			Assert.IsFalse(runnerMock.Token.IsCancellationRequested, "CancelationToken should not be canceled");

			await lifetimeMock.CompletionTask;

			Assert.IsTrue(runnerMock.Task!.IsFaulted, "Task should be faulted");

			lifetimeMock.ApplicationStoppingSource.Cancel();

			Assert.IsTrue(runnerMock.Token.IsCancellationRequested, "Task should be canceled");
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


		private class MockLifetime : IHostApplicationLifetime
		{
			public MockLifetime()
			{
				ApplicationStartedSource = new CancellationTokenSource();
				ApplicationStoppingSource = new CancellationTokenSource();
				ApplicationStoppedSource = new CancellationTokenSource();
				m_completionSource = new TaskCompletionSource<bool>();
			}


			public CancellationTokenSource ApplicationStartedSource { get; }


			public CancellationTokenSource ApplicationStoppingSource { get; }


			public CancellationTokenSource ApplicationStoppedSource { get; }


			public Task CompletionTask => m_completionSource.Task;


			public CancellationToken ApplicationStarted => ApplicationStartedSource.Token;


			public CancellationToken ApplicationStopping => ApplicationStoppingSource.Token;


			public CancellationToken ApplicationStopped => ApplicationStoppedSource.Token;


			public void StopApplication() => m_completionSource.SetResult(true);


			private readonly TaskCompletionSource<bool> m_completionSource;
		}
	}
}
