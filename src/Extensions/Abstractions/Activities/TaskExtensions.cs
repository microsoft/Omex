// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Abstractions.Activities
{
	/// <summary>
	/// Extensions for Task to wrap them in TimedScope
	/// </summary>
	public static class TaskExtensions
	{
		/// <summary>
		/// Starts TimedScopes before task execution and finish after it's completion
		/// </summary>
		public static async ValueTask<TResult> WithTimedScope<TResult>(this ValueTask<TResult> task, ActivitySource provider, string name)
		{
			using Activity? activity = provider.StartActivity(name);
			activity?.MarkAsSystemError();
			TResult result = await task.ConfigureAwait(false);
			activity?.MarkAsSuccess(); // set TimedScope result to success in case if task completed properly (without exception)
			return result;
		}

		/// <summary>
		/// Starts TimedScopes before task execution and finish after it's completion
		/// </summary>
		public static async ValueTask WithTimedScope(this ValueTask task, ActivitySource provider, string definition) =>
			// await required to convert ValueTask<T> to ValueTask https://github.com/microsoft/Omex/pull/153#discussion_r389761929
			await ConvertToTaskWithResultValue(task).WithTimedScope(provider, definition).ConfigureAwait(false);

		/// <summary>
		/// Starts TimedScopes before task execution and finish after it's completion
		/// </summary>
		public static ValueTask<TResult> WithTimedScope<TResult>(this Task<TResult> task, ActivitySource provider, string name) =>
			new ValueTask<TResult>(task).WithTimedScope(provider, name);

		/// <summary>
		/// Starts TimedScopes before task execution and finish after it's completion
		/// </summary>
		public static ValueTask WithTimedScope(this Task task, ActivitySource provider, string name) =>
			new ValueTask(task).WithTimedScope(provider, name);

		private static async ValueTask<bool> ConvertToTaskWithResultValue(ValueTask task)
		{
			await task.ConfigureAwait(false);
			return true;
		}
	}
}
