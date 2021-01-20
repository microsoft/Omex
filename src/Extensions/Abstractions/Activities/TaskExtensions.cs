// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Abstractions.Activities
{
	/// <summary>
	/// Extensions for Task to wrap them in Activity
	/// </summary>
	public static class TaskExtensions
	{
		/// <summary>
		/// Starts Activity before task execution and finish after it's completion
		/// </summary>
		public static async ValueTask<TResult> WithActivity<TResult>(this ValueTask<TResult> task, ActivitySource provider, string name)
		{
			using Activity? activity = provider.StartActivity(name);
			activity?.MarkAsSystemError();
			TResult result = await task.ConfigureAwait(false);
			activity?.MarkAsSuccess(); // set Activity result to success in case if task completed properly (without exception)
			return result;
		}

		/// <summary>
		/// Starts Activity before task execution and finish after it's completion
		/// </summary>
		public static async ValueTask WithActivity(this ValueTask task, ActivitySource provider, string definition) =>
			// await required to convert ValueTask<T> to ValueTask https://github.com/microsoft/Omex/pull/153#discussion_r389761929
			await ConvertToTaskWithResultValue(task).WithActivity(provider, definition).ConfigureAwait(false);

		/// <summary>
		/// Starts Activity before task execution and finish after it's completion
		/// </summary>
		public static ValueTask<TResult> WithActivity<TResult>(this Task<TResult> task, ActivitySource provider, string name) =>
			new ValueTask<TResult>(task).WithActivity(provider, name);

		/// <summary>
		/// Starts Activity before task execution and finish after it's completion
		/// </summary>
		public static ValueTask WithActivity(this Task task, ActivitySource provider, string name) =>
			new ValueTask(task).WithActivity(provider, name);

		private static async ValueTask<bool> ConvertToTaskWithResultValue(ValueTask task)
		{
			await task.ConfigureAwait(false);
			return true;
		}
	}
}
