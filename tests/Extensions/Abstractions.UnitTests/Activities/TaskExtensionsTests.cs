// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Testing.Helpers;
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

		private void TestSuccessfulExecution(Action<Task<bool>, ActivitySource, string> wrapTask, [CallerMemberName] string scopeName = "") =>
			TestExecution(scopeName, wrapTask, tcs => tcs.SetResult(true), ActivityResult.Success);

		private void TestFailedExecution(Action<Task<bool>, ActivitySource, string> wrapTask, [CallerMemberName] string scopeName = "") =>
			TestExecution(scopeName, wrapTask, tcs => tcs.SetException(new Exception("Some failure")), ActivityResult.SystemError);

		private void TestExecution(
			string scopeName,
			Action<Task<bool>, ActivitySource, string> createTask,
			Action<TaskCompletionSource<bool>> finishTask,
			ActivityResult expectedResult)
		{
			Mock<ActivitySource> providerMock = new Mock<ActivitySource>();
			Activity activity = new Activity(scopeName).MarkAsSystemError();

			providerMock.Setup(p => p.StartActivity(scopeName, ActivityKind.Internal)).Returns(activity);

			TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

			createTask(taskCompletionSource.Task, providerMock.Object, scopeName);

			providerMock.Verify(p => p.StartActivity(scopeName, ActivityKind.Internal), Times.Once);
			activity.AssertResult(ActivityResult.SystemError);

			finishTask(taskCompletionSource);

			activity.AssertResult(expectedResult);
		}

		private static Action<Task<bool>, ActivitySource, string> s_wrapTaskOfTAction =
			(task, provider, name) => task.WithTimedScope(provider, name).ConfigureAwait(false);

		private static Action<Task<bool>, ActivitySource, string> s_wrapTaskAction =
			(task, provider, name) => ((Task)task).WithTimedScope(provider, name).ConfigureAwait(false);

		private static Action<Task<bool>, ActivitySource, string> s_wrapValueTaskOfTAction =
			async (task, provider, name) => await new ValueTask<bool>(task).WithTimedScope(provider, name).ConfigureAwait(false);

		private static Action<Task<bool>, ActivitySource, string> s_wrapValueTaskAction =
			async (task, provider, name) => await new ValueTask(task).WithTimedScope(provider, name).ConfigureAwait(false);
	}
}
