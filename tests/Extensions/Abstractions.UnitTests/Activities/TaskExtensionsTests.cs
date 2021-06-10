// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.Omex.Extensions.Testing.Helpers.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Activities.UnitTests
{
	[TestClass]
	[TestCategory(nameof(Activity))]
	public class TaskExtensionsTests
	{
		[TestMethod]
		public void WithActivityWithTaskOfT_Success() => TestSuccessfulExecution(s_wrapTaskOfTAction);

		[TestMethod]
		public void WithActivityUsingTask_Success() => TestSuccessfulExecution(s_wrapTaskAction);

		[TestMethod]
		public void WithActivityUsingValueTaskOfT_Success() => TestSuccessfulExecution(s_wrapValueTaskOfTAction);

		[TestMethod]
		public void WithActivityUsingValueTask_Success() => TestSuccessfulExecution(s_wrapValueTaskAction);

		[TestMethod]
		public void WithActivityUsingTaskOfT_Failed() => TestFailedExecution(s_wrapTaskOfTAction);

		[TestMethod]
		public void WithActivityUsingTask_Failed() => TestFailedExecution(s_wrapTaskAction);

		[TestMethod]
		public void WithActivityUsingValueTaskOfT_Failed() => TestFailedExecution(s_wrapValueTaskOfTAction);

		[TestMethod]
		public void WithActivityUsingValueTask_Failed() => TestFailedExecution(s_wrapValueTaskAction);

		private void TestSuccessfulExecution(Action<Task<bool>, ActivitySource, string> wrapTask, [CallerMemberName] string activityName = "") =>
			TestExecution(activityName, wrapTask, tcs => tcs.SetResult(true), ActivityResult.Success);

		private void TestFailedExecution(Action<Task<bool>, ActivitySource, string> wrapTask, [CallerMemberName] string activityName = "") =>
			TestExecution(activityName, wrapTask, tcs => tcs.SetException(new DivideByZeroException("Some failure")), ActivityResult.SystemError);

		private void TestExecution(
			string activityName,
			Action<Task<bool>, ActivitySource, string> createTask,
			Action<TaskCompletionSource<bool>> finishTask,
			ActivityResult expectedResult)
		{
			using TestActivityListener listener = new TestActivityListener(activityName);
			ActivitySource source = new ActivitySource(activityName);

			TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

			createTask(taskCompletionSource.Task, source, activityName);

			Activity? activity = listener.Activities.Single();

			NullableAssert.IsNotNull(activity);

			activity.AssertResult(ActivityResult.SystemError);

			finishTask(taskCompletionSource);

			activity.AssertResult(expectedResult);
		}

		private static Action<Task<bool>, ActivitySource, string> s_wrapTaskOfTAction =
			async (task, provider, name) =>
				await CatchExceptionAsync(async () => await task.WithActivity(provider, name).ConfigureAwait(false))
					.ConfigureAwait(false);

		private static Action<Task<bool>, ActivitySource, string> s_wrapTaskAction =
			async (task, provider, name) =>
				await CatchExceptionAsync(async () => await ((Task)task).WithActivity(provider, name).ConfigureAwait(false))
					.ConfigureAwait(false);

		private static Action<Task<bool>, ActivitySource, string> s_wrapValueTaskOfTAction =
			async (task, provider, name) =>
				await CatchExceptionAsync(async () => await new ValueTask<bool>(task).WithActivity(provider, name).ConfigureAwait(false))
					.ConfigureAwait(false);

		private static Action<Task<bool>, ActivitySource, string> s_wrapValueTaskAction =
			async (task, provider, name) =>
				await CatchExceptionAsync(async () => await new ValueTask(task).WithActivity(provider, name).ConfigureAwait(false))
					.ConfigureAwait(false);

		private static async Task CatchExceptionAsync(Func<Task> func)
		{
			try
			{
				await func().ConfigureAwait(false);
			}
			catch (DivideByZeroException)
			{
				// ignore produced by test
			}
		}
	}
}
