// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	[TestCategory(nameof(Activity))]
	public class TaskExtensionsTests
	{
		[TestMethod]
		public void WithTimedScopeWithTaskOfT_Success() => TestSuccessfulExecution(s_wrapTaskOfTAction);


		[TestMethod]
		public void WithTimedScopeUsingTask_Success() => TestSuccessfulExecution(s_wrapTaskAction);


		[TestMethod]
		public void WithTimedScopeUsingValueTaskOfT_Success() => TestSuccessfulExecution(s_wrapValueTaskOfTAction);


		[TestMethod]
		public void WithTimedScopeUsingValueTask_Success() => TestSuccessfulExecution(s_wrapValueTaskAction);


		[TestMethod]
		public void WithTimedScopeUsingTaskOfT_Failed() => TestFailedExecution(s_wrapTaskOfTAction);


		[TestMethod]
		public void WithTimedScopeUsingTask_Failed() => TestFailedExecution(s_wrapTaskAction);


		[TestMethod]
		public void WithTimedScopeUsingValueTaskOfT_Failed() => TestFailedExecution(s_wrapValueTaskOfTAction);


		[TestMethod]
		public void WithTimedScopeUsingValueTask_Failed() => TestFailedExecution(s_wrapValueTaskAction);


		private void TestSuccessfulExecution(Action<Task<bool>, ITimedScopeProvider, TimedScopeDefinition> wrapTask, [CallerMemberName] string scopeName = "") =>
			TestExecution(scopeName, wrapTask, tcs => tcs.SetResult(true), TimedScopeResult.Success);


		private void TestFailedExecution(Action<Task<bool>, ITimedScopeProvider, TimedScopeDefinition> wrapTask, [CallerMemberName] string scopeName = "") =>
			TestExecution(scopeName, wrapTask, tcs => tcs.SetException(new Exception("Some failure")), TimedScopeResult.SystemError);


		private void TestExecution(
			string scopeName,
			Action<Task<bool>, ITimedScopeProvider, TimedScopeDefinition> createTask,
			Action<TaskCompletionSource<bool>> finishTask,
			TimedScopeResult expectedResult)
		{
			Mock<ITimedScopeProvider> providerMock = new Mock<ITimedScopeProvider>();
			TimedScopeDefinition timedScopeDefinition = new TimedScopeDefinition(scopeName);
			Activity activity = new Activity(scopeName);

			TimedScope scope = new TimedScope(activity, TimedScopeResult.SystemError);
			providerMock.Setup(p => p.CreateAndStart(timedScopeDefinition, TimedScopeResult.SystemError)).Returns(scope);

			TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

			createTask(taskCompletionSource.Task, providerMock.Object, timedScopeDefinition);

			providerMock.Verify(p => p.CreateAndStart(timedScopeDefinition, TimedScopeResult.SystemError), Times.Once);
			scope.AssertResult(TimedScopeResult.SystemError);

			finishTask(taskCompletionSource);

			scope.AssertResult(expectedResult);
		}


		private static Action<Task<bool>, ITimedScopeProvider, TimedScopeDefinition> s_wrapTaskOfTAction =
			(task, provider, name) => task.WithTimedScope(provider, name).ConfigureAwait(false);


		private static Action<Task<bool>, ITimedScopeProvider, TimedScopeDefinition> s_wrapTaskAction =
			(task, provider, name) => ((Task)task).WithTimedScope(provider, name).ConfigureAwait(false);


		private static Action<Task<bool>, ITimedScopeProvider, TimedScopeDefinition> s_wrapValueTaskOfTAction =
			async (task, provider, name) => await new ValueTask<bool>(task).WithTimedScope(provider, name).ConfigureAwait(false);


		private static Action<Task<bool>, ITimedScopeProvider, TimedScopeDefinition> s_wrapValueTaskAction =
			async (task, provider, name) => await new ValueTask(task).WithTimedScope(provider, name).ConfigureAwait(false);
	}
}
